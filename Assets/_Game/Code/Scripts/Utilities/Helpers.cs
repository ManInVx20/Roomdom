using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace VinhLB.Utilities
{
    public static class VLBApplication
    {
        public enum RunMode
        {
            Device = 0,
            Editor = 1,
            Simulator = 2
        }

        public static RunMode CurrentRunMode
        {
            get
            {
#if UNITY_EDITOR
                return UnityEngine.Device.Application.isEditor && !UnityEngine.Device.Application.isMobilePlatform
                    ? RunMode.Editor
                    : RunMode.Simulator;
#else
                return RunMode.Device;
#endif
            }
        }

        public static bool IsOnEditor()
        {
            return CurrentRunMode is RunMode.Editor or RunMode.Simulator;
        }
    }

    public static class VLBRendering
    {
        public enum RenderPipelineMode
        {
            Unsupported = 0,
            BuiltIn = 1,
            Universal = 2,
            HighDefinition = 3
        }

        public static RenderPipelineMode CurrentRenderPipelineMode
        {
            get
            {
#if UNITY_2019_1_OR_NEWER
                if (GraphicsSettings.renderPipelineAsset != null)
                {
                    // SRP
                    var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
                    if (srpType.Contains("HDRenderPipelineAsset"))
                    {
                        return RenderPipelineMode.HighDefinition;
                    }

                    if (srpType.Contains("UniversalRenderPipelineAsset") ||
                        srpType.Contains("LightweightRenderPipelineAsset"))
                    {
                        return RenderPipelineMode.Universal;
                    }

                    return RenderPipelineMode.Unsupported;
                }
#elif UNITY_2017_1_OR_NEWER
                    if (GraphicsSettings.renderPipelineAsset != null)
                    {
                        // SRP is not supported before 2019
                        return RenderPipelineType.Unsupported;
                    }
#endif
                // No SRP
                return RenderPipelineMode.BuiltIn;
            }
        }
    }

    public static class VLBInput
    {
        public struct TouchData
        {
            public int index;
            public Vector3 position;
        }

        public static Vector2 GetPointerDeltaPosition(int touchIndex = 0)
        {
            Vector2 deltaPosition = Vector2.zero;
            if (VLBApplication.IsOnEditor())
            {
                if (Input.GetMouseButton(0))
                {
                    deltaPosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                }
            }
            else
            {
                if (Input.touchCount > 0 && touchIndex < Input.touchCount)
                {
                    deltaPosition = Input.GetTouch(touchIndex).deltaPosition;
                    deltaPosition.x /= Screen.width;
                    deltaPosition.y /= Screen.height;
                    deltaPosition *= 100f;
                }
            }

            return deltaPosition;
        }

        public static Vector3 GetPointerScreenPosition(int touchIndex = 0)
        {
            Vector3 pointerScreenPosition = Vector3.zero;
            if (VLBApplication.IsOnEditor())
            {
                if (Input.mousePresent)
                {
                    pointerScreenPosition = Input.mousePosition;
                }
            }
            else
            {
                if (Input.touchCount > 0 && touchIndex < Input.touchCount)
                {
                    pointerScreenPosition = Input.GetTouch(touchIndex).position;
                }
            }

            return pointerScreenPosition;
        }

        public static Vector3 GetPointerWorldPosition(Camera camera = null, int touchIndex = 0, float zPosition = 0)
        {
            Vector3 pointerScreenPosition = GetPointerScreenPosition(touchIndex);
            pointerScreenPosition.z = zPosition;
            Vector3 pointerWorldPosition = VLBConversion.ScreenToWorldPosition(pointerScreenPosition, camera);

            return pointerWorldPosition;
        }

        public static bool IsPointerOnUI(int touchIndex = 0)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = GetPointerScreenPosition(touchIndex);

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            for (int i = 0; i < raycastResults.Count; i++)
            {
                if (raycastResults[i].gameObject.layer == VLBLayer.UI)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsPointerActive()
        {
            bool result;
            if (VLBApplication.IsOnEditor())
            {
                result = Input.GetMouseButton(0);
            }
            else
            {
                result = Input.touchCount > 0;
            }

            return result;
        }

        public static bool IsPointerDown()
        {
            bool result;
            if (VLBApplication.IsOnEditor())
            {
                result = Input.GetMouseButtonDown(0);
            }
            else
            {
                result = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
            }

            return result;
        }

        public static bool IsPointerUp()
        {
            bool result;
            if (VLBApplication.IsOnEditor())
            {
                result = Input.GetMouseButtonUp(0);
            }
            else
            {
                result = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
            }

            return result;
        }

        public static bool IsAnyPointerDown(out List<TouchData> touchDataList)
        {
            touchDataList = new List<TouchData>();
            bool result = false;
            if (VLBApplication.IsOnEditor())
            {
                result = Input.GetMouseButtonDown(0);
            }
            else
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began)
                    {
                        touchDataList.Add(new TouchData()
                        {
                            index = i,
                            position = touch.position
                        });
                        result = true;
                    }
                }
            }

            return result;
        }

        public static bool IsAnyPointerUp(out List<TouchData> touchDataList)
        {
            touchDataList = new List<TouchData>();
            bool result = false;
            if (VLBApplication.IsOnEditor())
            {
                result = Input.GetMouseButtonUp(0);
            }
            else
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Ended)
                    {
                        touchDataList.Add(new TouchData()
                        {
                            index = i,
                            position = touch.position
                        });
                        result = true;
                    }
                }
            }

            return result;
        }
    }

    public static class VLBLayer
    {
        public static readonly int Default = LayerMask.NameToLayer("Default");
        public static readonly int IgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        public static readonly int UI = LayerMask.NameToLayer("UI");
        public static readonly int GameUI = LayerMask.NameToLayer("GameUI");
        // public static readonly int Draggable = LayerMask.NameToLayer("Draggable");
        // public static readonly int Slot = LayerMask.NameToLayer("Slot");
        // public static readonly int Wall = LayerMask.NameToLayer("Wall");
        // public static readonly int Interactable = LayerMask.NameToLayer("Interactable");

        public static LayerMask GetLayerMask(int layer)
        {
            return (1 << layer);
        }

        public static LayerMask GetPhysicsLayerMask(int layer)
        {
            int layerMask = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(layer, i))
                {
                    layerMask |= (1 << i);
                }
            }

            return layerMask;
        }

        public static LayerMask GetPhysics2DLayerMask(int layer)
        {
            int layerMask = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics2D.GetIgnoreLayerCollision(layer, i))
                {
                    layerMask |= (1 << i);
                }
            }

            return layerMask;
        }
    }

    public static class VLBPhysics
    {
        public static bool TryCastRay(Camera camera, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            return TryCastRay(0, camera, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        }

        public static bool TryCastRay(int touchIndex, Camera camera, out RaycastHit hitInfo,
            float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
        {
            Vector3 pointerScreenPosition = VLBInput.GetPointerScreenPosition();

            Vector3 pointerScreenPositionFar = pointerScreenPosition;
            pointerScreenPositionFar.z = camera.farClipPlane;
            Vector3 pointerScreenPositionNear = pointerScreenPosition;
            pointerScreenPositionNear.z = camera.nearClipPlane;

            Vector3 pointerWorldPositionFar = camera.ScreenToWorldPoint(pointerScreenPositionFar);
            Vector3 pointerWorldPositionNear = camera.ScreenToWorldPoint(pointerScreenPositionNear);

            return Physics.Raycast(pointerWorldPositionNear, pointerWorldPositionFar - pointerWorldPositionNear,
                out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        }
    }

    public static class VLBConversion
    {
        public static Vector3 ScreenToWorldPosition(Vector3 screenPosition, Camera camera = null)
        {
            Vector3 pointerWorldPosition = Vector3.zero;
            camera ??= Camera.main;
            if (camera != null)
            {
                pointerWorldPosition = camera.ScreenToWorldPoint(screenPosition);
            }

            return pointerWorldPosition;
        }
    }
}