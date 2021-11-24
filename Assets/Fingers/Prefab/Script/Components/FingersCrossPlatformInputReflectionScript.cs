//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System;
using System.Reflection;

using UnityEngine;

namespace DigitalRubyShared
{
    /// <summary>
    /// Allow interacting with cross platform input without knowing whether the cross platform input script is in the project
    /// </summary>
    public static class FingersCrossPlatformInputReflectionScript
    {
        private static readonly Type crossPlatformInputManagerType;
        private static readonly Type virtualAxisType;
        private static readonly MethodInfo registerVirtualAxisMethod;
        private static readonly MethodInfo unRegisterVirtualAxisMethod;
        private static readonly MethodInfo getVirtualAxisMethod;
        private static readonly MethodInfo axisUpdateMethod;

        private static Type GetCrossPlatformInputType(string subType)
        {
            Type type = null;

#if !PLATFORM_METRO

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    type = assembly.GetType("UnityStandardAssets.CrossPlatformInput." + subType);
                    if (type == null)
                    {
                        type = assembly.GetType("UnitySampleAssets.CrossPlatformInput." + subType);
                    }
                    if (type != null)
                    {
                        break;
                    }
                }
                catch
                {
                }
            }

#endif

            return type;
        }

        /// <summary>
        /// Setup reflection for cross platform input manager
        /// </summary>
        static FingersCrossPlatformInputReflectionScript()
        {
            crossPlatformInputManagerType = GetCrossPlatformInputType("CrossPlatformInputManager");
            if (crossPlatformInputManagerType != null)
            {
                virtualAxisType = GetCrossPlatformInputType("CrossPlatformInputManager+VirtualAxis");
                if (virtualAxisType == null)
                {
                    Debug.LogWarning("Unable to get 'UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager+VirtualAxis' type");
                }
                else
                {
                    registerVirtualAxisMethod = crossPlatformInputManagerType.GetMethod("RegisterVirtualAxis", BindingFlags.Public | BindingFlags.Static);
                    if (registerVirtualAxisMethod == null)
                    {
                        Debug.LogWarning("Unable to get 'CrossPlatformInputManager.RegisterVirtualAxis' method");
                    }
                    else
                    {
                        unRegisterVirtualAxisMethod = crossPlatformInputManagerType.GetMethod("UnRegisterVirtualAxis", BindingFlags.Public | BindingFlags.Static);
                        if (unRegisterVirtualAxisMethod == null)
                        {
                            Debug.LogWarning("Unable to get 'CrossPlatformInputManager.UnregisterVirtualAxis' method");
                        }
                        else
                        {
                            getVirtualAxisMethod = crossPlatformInputManagerType.GetMethod("VirtualAxisReference", BindingFlags.Public | BindingFlags.Static);
                            if (getVirtualAxisMethod == null)
                            {
                                Debug.LogWarning("Unable to get 'CrossPlatformInputManager.VirtualAxisReference' method");
                            }
                            else
                            {
                                axisUpdateMethod = virtualAxisType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
                                if (axisUpdateMethod == null)
                                {
                                    Debug.LogWarning("Unable to get 'VirtualAxis.Update' method");
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Register a virtual axis
        /// </summary>
        /// <param name="name">Virtual axis name</param>
        /// <param name="registered">True if the object was registered, false if it already existed</param>
        /// <returns>Virtual axis object or null if error</returns>
        public static object RegisterVirtualAxis(string name, out bool registered)
        {
            registered = false;
            if (registerVirtualAxisMethod == null)
            {
                return null;
            }
            object result = GetVirtualAxis(name);
            if (result == null)
            {
                result = Activator.CreateInstance(virtualAxisType, name);
                // if constructor didn't register, do so now
                if (GetVirtualAxis(name) == null)
                {
                    registerVirtualAxisMethod.Invoke(null, new object[] { result });
                }
                registered = true;
            }
            return result;
        }

        /// <summary>
        /// Unregister a virtual axis
        /// </summary>
        /// <param name="name">Virtual axis name</param>
        /// <returns>True if success, false if error</returns>
        public static bool UnRegisterVirtualAxis(string name)
        {
            if (unRegisterVirtualAxisMethod == null)
            {
                return false;
            }
            unRegisterVirtualAxisMethod.Invoke(null, new object[] { name });
            return true;
        }

        /// <summary>
        /// Get a virtual axis if it exists
        /// </summary>
        /// <param name="name">Name of virtual axis</param>
        /// <returns>Virtual axis object or null if not exists or error</returns>
        public static object GetVirtualAxis(string name)
        {
            if (getVirtualAxisMethod == null)
            {
                return null;
            }
            return getVirtualAxisMethod.Invoke(null, new object[] { name });
        }

        /// <summary>
        /// Update a virtual axis value
        /// </summary>
        /// <param name="axis">Axis object returned from RegisterVirtualAxis</param>
        /// <param name="value">New value</param>
        /// <returns>True if success, false if error</returns>
        public static bool UpdateVirtualAxis(object axis, float value)
        {
            if (axisUpdateMethod == null)
            {
                return false;
            }
            axisUpdateMethod.Invoke(axis, new object[] { value });
            return true;
        }
    }
}
