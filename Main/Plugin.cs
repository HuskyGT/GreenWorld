using BepInEx;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utilla;

namespace GreenWorld
{
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        bool InModdedRoom;

        void Awake()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
        }

        void OnEnable()
        {
        }

        void OnDisable()
        {
            ResetAllRenderers();
        }

        Transform pointer;
        Material greenMat;
        void OnGameInitialized(object sender, EventArgs e)
        {
            gameObject.AddComponent<VRInput>();
            pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            pointer.gameObject.GetComponent<MeshRenderer>().material.shader = GorillaTagger.Instance.offlineVRRig.materialsToChangeTo[0].shader;
            pointer.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
            greenMat = pointer.gameObject.GetComponent<MeshRenderer>().material;
            pointer.localScale = new Vector3(0.03f, 0.03f, 0.03f);
            Destroy(pointer.GetComponent<Collider>());
        }

        Dictionary<Renderer, Material> alteredRenderersDict = new Dictionary<Renderer, Material>();


        void ResetAllRenderers()
        {
            foreach (KeyValuePair<Renderer, Material> keypair in alteredRenderersDict)
            {
                keypair.Key.material = keypair.Value;
            }
            alteredRenderersDict.Clear();
        }
        void Update()
        {
            if (pointer == null)
                return;

            if (!InModdedRoom)
                return;

            if (VRInput.rightHand.trigger.Released)
            {
                pointer.position = Vector3.zero;
            }

            if (!VRInput.rightHand.trigger.Held)
                return;

            if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightControllerTransform.position, GorillaLocomotion.Player.Instance.rightControllerTransform.forward, out var hitInfo, 9999f))
            {
                pointer.position = hitInfo.point;
                if (VRInput.rightHand.primary.Pressed)
                {
                    foreach (var renderer in hitInfo.collider.gameObject.transform.parent.GetComponentsInChildren<Renderer>())
                    {
                        if (alteredRenderersDict.ContainsKey(renderer))
                            return;

                        alteredRenderersDict.Add(renderer, renderer.sharedMaterial);
                        renderer.material = greenMat;
                    }
                }
            }
        }


        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            InModdedRoom = true;
        }

        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            InModdedRoom = false;
            ResetAllRenderers();
        }
    }
}
