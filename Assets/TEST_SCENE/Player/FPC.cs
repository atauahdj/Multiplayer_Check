using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float touchSensitivity = 0.1f;
    public Transform cameraPivot;
    public Transform hand;
    public GameObject modelRoot;
    public Rigidbody[] Bones;
    private float rotationX = 0f;
    private float rotationY = 0f;
    private Rigidbody rb;
    private GameObject currentItem; // Предмет в руках
    public bool isRagdoll;
    public Animator anm;
    public NetworkManager network;
    void Start()
    {
        if (!isLocalPlayer)
        {
            if (cameraPivot != null) cameraPivot.gameObject.SetActive(false);
            return;
        }
        GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");
        if (canvas != null && !canvas.activeSelf) canvas.SetActive(true);
        network = FindAnyObjectByType<NetworkManager>();
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        foreach (AudioListener al in GameObject.FindObjectsOfType<AudioListener>())
        {
            if (isLocalPlayer) return;
            al.enabled = false;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (modelRoot != null) modelRoot.SetActive(false);
        transform.position = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
    }
    bool pause;
    void Update()
    {
        if (!isLocalPlayer) return;
        HandleCamera();
        UpItem();
        anm.transform.position = new Vector3(transform.position.x, transform.position.y -1, transform.position.z);
        if (isRagdoll)
        {
            anm.enabled = false;
            anm.transform.rotation = anm.transform.rotation;
            foreach (Rigidbody bone in Bones)
            {
                if(bone.isKinematic == true)
                {
                    bone.isKinematic = false;
                    bone.GetComponent<Collider>().enabled = true;
                }
            }
        }
        else
        {
            anm.enabled=true;
            anm.transform.rotation = transform.rotation;
            foreach (Rigidbody bone in Bones)
            {
                if( bone.isKinematic == false)
                {
                    bone.isKinematic = true;
                    bone.GetComponent<Collider>().enabled = false;
                }
            }
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame && !pause)
        {
            Cursor.lockState = CursorLockMode.None;
            pause = true;
        }
        else if(pause && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            pause = false;
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer || rb == null || isRagdoll) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 moveDir = (transform.right * moveX + transform.forward * moveZ).normalized;

        if (moveDir != Vector3.zero)
        {
            Vector3 targetVelocity = moveDir * (Keyboard.current.leftShiftKey.isPressed ? runSpeed : moveSpeed);
            targetVelocity.y = rb.velocity.y;
            rb.velocity = targetVelocity;
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    private void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * touchSensitivity * 10f;
        float mouseY = Input.GetAxis("Mouse Y") * touchSensitivity * 10f;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
        if (cameraPivot != null) cameraPivot.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }

    // ================== ЛОГИКА ПРЕДМЕТОВ ==================

    // Выполняется на СЕРВЕРЕ
    [Command]
    void CmdGiveAuthority(NetworkIdentity itemIdentity)
    {
        if (itemIdentity == null) return;

        // Если у предмета уже есть владелец (и это не мы), забираем у него права
        if (itemIdentity.connectionToClient != null && itemIdentity.connectionToClient != connectionToClient)
        {
            itemIdentity.RemoveClientAuthority();
        }

        // Выдаем права нашему клиенту
        itemIdentity.AssignClientAuthority(connectionToClient);
    }

    // Выполняется на СЕРВЕРЕ
    [Command]
    void CmdRemoveAuthority(NetworkIdentity itemIdentity)
    {
        if (itemIdentity == null) return;

        // Если предмет кому-то принадлежит, снимаем права
        if (itemIdentity.connectionToClient != null)
        {
            itemIdentity.RemoveClientAuthority();
        }
    }

    private void UpItem()
    {
        // 1. Если в руках ничего нет — пытаемся подобрать
        if (currentItem == null)
        {
            Ray ray = new Ray(cameraPivot.position, cameraPivot.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 5f))
            {
                if (hit.collider.CompareTag("Item") && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    NetworkIdentity itemNet = hit.collider.GetComponent<NetworkIdentity>();

                    if (itemNet != null && itemNet.connectionToClient == null) // Проверяем, что предмет свободен
                    {
                        CmdGiveAuthority(itemNet); // Просим сервер дать права
                        currentItem = hit.collider.gameObject; // Берем в руки локально

                        // Отключаем физику, чтобы предмет не вырывался из рук
                        if (currentItem.TryGetComponent<Rigidbody>(out Rigidbody itemRb)) itemRb.isKinematic = true;
                        if (currentItem.TryGetComponent<Collider>(out Collider itemCol)) itemCol.enabled = false;
                    }
                }
                else if((hit.collider.CompareTag("Door") && Keyboard.current.eKey.wasPressedThisFrame))
                {
                    hit.collider.GetComponent<Open_Door_Script>().Open();
                }
            }
        }

        // 2. Если в руках есть предмет — держим его и слушаем кнопку выброса
        if (currentItem != null)
        {
            // Двигаем предмет в руке (ТОЛЬКО если у нас есть права)
            NetworkIdentity itemNet = currentItem.GetComponent<NetworkIdentity>();
            if (itemNet != null && itemNet.isOwned)
            {
                currentItem.transform.rotation = cameraPivot.rotation;
                currentItem.transform.position = hand.position;
            }

            // Выброс
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                CmdRemoveAuthority(itemNet); // Просим сервер снять права

                // Включаем физику обратно
                if (currentItem.TryGetComponent<Rigidbody>(out Rigidbody itemRb)) itemRb.isKinematic = false;
                if (currentItem.TryGetComponent<Collider>(out Collider itemCol)) itemCol.enabled = true;

                currentItem = null; // Очищаем руки
            }
        }
    }
    [ClientRpc]
    public void Die()
    {
        isRagdoll = true;
        if (isRagdoll)
        {
            anm.enabled = false;
            anm.transform.rotation = anm.transform.rotation;
            foreach (Rigidbody bone in Bones)
            {
                if (bone.isKinematic == true)
                {
                    bone.isKinematic = false;
                    bone.GetComponent<Collider>().enabled = true;
                }
            }
        }
        cameraPivot.GetComponent<Light>().enabled = false;
        modelRoot.SetActive(true);
        modelRoot = null;
        gameObject.tag = "Untagged";
        Cursor.lockState = CursorLockMode.None;
    }
}