using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public bool isGrabbing { get; private set; }

    [SerializeField] private Player player;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform camProxy;

    [SerializeField] private NetworkObject grabbedObject;
    [SerializeField] private Transform grabPos;
    [SerializeField] private float grabVelocityThreshold;

    [SerializeField] private float velocityMultiplier;
    [SerializeField] private float currentCharge;
    [SerializeField] private float throwThreshold;
    [SerializeField] private float throwForce;
    [SerializeField] private float throwOffset;
    [SerializeField] private float dropOffset;

    public float GetGrabVelocityThreshold()
    {
        return grabVelocityThreshold;
    }

    private void Update()
    {
        Quaternion camRot = Quaternion.LookRotation(camProxy.forward, Vector3.up);
        camRot = Quaternion.Euler(new Vector3(0, camRot.eulerAngles.y, 0));
        Vector3 dropPos = (camRot * transform.forward).normalized * 0.65f;
        dropPos.y = -0.25f;

        grabPos.localPosition = dropPos;
        grabPos.rotation = camRot;

        if (!isGrabbing)
            return;

    }

    public void InputReceived(int index, Vector3 position)
    {
        if (NetworkObject.list.Count >= NetworkManager.Singleton.ObjectCap)
            return;

        NetworkObject.Spawn(index, position);
    }

    public void LeftClick(bool isHeld)
    {
        //attempt to throw
        if (!isGrabbing)
            return;

        if (isHeld)
        {
            currentCharge += Time.fixedDeltaTime;
        }
        else
        {
            if(currentCharge > 0)
            {
                ushort objectId = grabbedObject.GetComponent<NetworkObject>().Id;

                ToggleGrabbedObject(false);
                if (currentCharge > throwThreshold)
                {
                    //Quaternion camRot = Quaternion.LookRotation(camProxy.forward, Vector3.up);
                    //camRot = Quaternion.Euler(new Vector3(0, camRot.eulerAngles.y, 0));

                    //Vector3 dropPos = (camRot * transform.forward).normalized * throwOffset;
                    Vector3 dropPos = camProxy.forward.normalized * throwOffset;
                    dropPos.y *= 1.35f;
                    dropPos.y = Mathf.Max(dropPos.y, 0f);
                    grabbedObject.transform.position = dropPos + transform.localPosition;

                    if (grabbedObject.TryGetComponent<Rigidbody>(out Rigidbody objRb))
                    {
                        objRb.velocity = camProxy.forward * ((Mathf.Min(currentCharge, 0.8f) / 0.8f)* throwForce) + camProxy.forward * new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude * velocityMultiplier; // (movement.MoveDirection.magnitude * moveDirectionMultipier)
                        objRb.angularVelocity = new Vector3(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
                    }
                }
                else
                {
                    Quaternion camRot = Quaternion.LookRotation(camProxy.forward, Vector3.up);
                    camRot = Quaternion.Euler(new Vector3(0, camRot.eulerAngles.y, 0));

                    Vector3 dropPos = (camRot * transform.forward).normalized * dropOffset;
                    dropPos.y = 0f;
                    grabbedObject.transform.position = dropPos + transform.localPosition;

                    //if (grabbedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
                       //rb.velocity = camProxy.forward * (movement.MoveDirection.magnitude * moveDirectionMultipier);
                }
                grabbedObject.transform.SetParent(null);
                grabbedObject = null;

                UpdateGrabbedObject();
            }
            currentCharge = 0;
        }
    }

    public void RightClick(ushort id) //this parameter is the clicked object's id
    {
        //attempt to grab 
        if(NetworkObject.list.TryGetValue(id, out NetworkObject obj))
        {
            if (Vector3.Distance(obj.transform.position, transform.position) > 5 || obj.transform.parent != null || obj.GetComponent<Rigidbody>().velocity.magnitude > grabVelocityThreshold)
                return;
        }

        if (grabbedObject != null) // haha this code is shit and doodoo and i hate it
        {//basically replaces the current grabbed object
            grabbedObject.transform.position = obj.transform.position;
            grabbedObject.transform.SetParent(null);
            ToggleGrabbedObject(false);
        }

        grabbedObject = obj;
        ToggleGrabbedObject(true);

        isGrabbing = true;

        Quaternion camRot = Quaternion.LookRotation(camProxy.forward, Vector3.up);
        camRot = Quaternion.Euler(new Vector3(0, camRot.eulerAngles.y, 0));
        grabbedObject.transform.position = grabPos.position;
        grabbedObject.transform.rotation = camRot;
        grabbedObject.transform.SetParent(grabPos);

        UpdateGrabbedObject();
    }

    private void ToggleGrabbedObject(bool a)
    {
        if (grabbedObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.isKinematic = a;
        if (grabbedObject.TryGetComponent<Collider>(out Collider col))
            col.enabled = !a;
        isGrabbing = a;
    }

    private void UpdateGrabbedObject()
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.grabbedObject);
        message.AddUShort(player.Id);
        message.AddInt(grabbedObject != null ?  grabbedObject.Id : -1);
        NetworkManager.Singleton.Server.Send(message, player.Id);
    }

    public void Hammer(ushort id)
    {
        if (!NetworkObject.list.TryGetValue(id, out NetworkObject obj))
            return;
        if (Vector3.Distance(obj.transform.position, transform.position) > 5 || obj.transform.parent != null || obj.GetComponent<Rigidbody>().velocity.magnitude > grabVelocityThreshold)
            return;

        obj.DestroyObject();
    }
}
