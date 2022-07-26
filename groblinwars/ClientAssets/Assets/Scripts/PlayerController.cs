using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform camTransform;

    [SerializeField] private float grabDistance;
    [SerializeField] private LayerMask grabMask;
    [SerializeField] private Transform grabPos;

    [SerializeField] private GameObject grabbedObjectGraphic;

    //[SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject canGrabCrosshair;
    private NetworkObject grabbedObject;
    [SerializeField] private Slider chargeBar;

    private bool[] inputs;
    private bool leftClick = false;
    private float chargeTime = 0;

    private void Start()
    {
        inputs = new bool[10]; 
    }

    private void Update()
    {
        //adaptive crosshair
        AdaptiveCrosshair();
        if (chargeTime > 0.15f)
            chargeBar.value = chargeTime / 0.8f;
        else
            chargeBar.value = 0;

        //movement inputs
        if (Input.GetKey(KeyCode.W))
            inputs[0] = true;
        if (Input.GetKey(KeyCode.S))
            inputs[1] = true;
        if (Input.GetKey(KeyCode.A))
            inputs[2] = true;
        if (Input.GetKey(KeyCode.D))
            inputs[3] = true;
        if (Input.GetKey(KeyCode.Space))
            inputs[4] = true;
        if (Input.GetKey(KeyCode.LeftShift))
            inputs[5] = true;
        if (Input.GetKey(KeyCode.LeftControl))
            inputs[6] = true;

        /*if (Input.GetKeyDown(KeyCode.E))
            SendInteractiveInput(0);
        if (Input.GetKeyDown(KeyCode.Q))
            SendInteractiveInput(1);*/

        if (Input.GetMouseButton(0))
            chargeTime += Time.deltaTime;
        if (Input.GetMouseButtonUp(0))
        {
            SendLeftClick();
            chargeTime = 0;
        }
        if (Input.GetMouseButtonDown(1))
            SendRightClick();

        if(grabbedObject == null)
            grabbedObjectGraphic.SetActive(false);
    }

    private void FixedUpdate()
    {
        SendMovementInput();

        for (int i = 0; i < inputs.Length; i++)
            inputs[i] = false;
        leftClick = false;
    }

    public void UpdateGrabObject(int id)
    {
        if (id == -1)
        {
            grabbedObject.showGraphics = true;
            grabbedObjectGraphic.SetActive(false);
        } 
        else
        {
            NetworkObject.list.TryGetValue(Convert.ToUInt16(id), out NetworkObject obj);

            if (grabbedObject != null)
                grabbedObject.showGraphics = true;

            grabbedObject = obj;
            grabbedObject.showGraphics = false;

            grabbedObjectGraphic.SetActive(true);
            grabbedObjectGraphic.GetComponent<MeshFilter>().mesh = grabbedObject.GetComponent<MeshFilter>().mesh;
            grabbedObjectGraphic.GetComponent<MeshRenderer>().material = grabbedObject.GetComponent<MeshRenderer>().material;
            grabbedObjectGraphic.transform.localScale = grabbedObject.transform.localScale;
        }
    }

    private void AdaptiveCrosshair()
    {
        Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit, grabDistance, grabMask);
        if(hit.collider != null)
        {
            canGrabCrosshair.SetActive(true);
            return;
        }
        canGrabCrosshair.SetActive(false);
    }

    #region Messages
    private void SendMovementInput()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ClientToServerId.movementInput);
        message.AddBools(inputs, false);
        message.AddVector3(camTransform.forward);
        NetworkManager.Singleton.Client.Send(message);
    }

    private void SendInteractiveInput(int index)
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.interactiveInput);
        message.AddInt(index);
        message.AddVector3(transform.position);
        NetworkManager.Singleton.Client.Send(message);
    }

    private void SendLeftClick()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.leftClick);
        message.AddFloat(chargeTime);
        NetworkManager.Singleton.Client.Send(message);
    }

    private void SendHammer()
    {
        Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit, grabDistance, grabMask);
        if (hit.collider != null)
        {
            Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.hammer);
            message.AddUShort(hit.collider.GetComponent<NetworkObject>().Id);
            NetworkManager.Singleton.Client.Send(message);
        }
    }

    private void SendRightClick()
    {
        Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit, grabDistance, grabMask);
        if(hit.collider != null)
        {
            Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.rightClick);
            message.AddUShort(hit.collider.GetComponent<NetworkObject>().Id);
            NetworkManager.Singleton.Client.Send(message);
        }
    }
    #endregion
}