﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VehicleEffects.GameExtensions;

namespace VehicleEffects.Effects
{
    public class CustomVehicleEffect
    {
        public static GameObject gameObject { get; private set; }
        private const string effectName = "Vehicle Effect Wrapper";
        private static Dictionary<String, EffectInfo> modifiedEffects;


        public static GameObject CreateEffectObject(Transform parent)
        {
            if(gameObject != null)
            {
                Debug.LogWarning("Creating effect object for " + effectName + " but object already exists!");
            }

            modifiedEffects = new Dictionary<string, EffectInfo>();
            gameObject = new GameObject(effectName + " Effects");
            gameObject.transform.parent = parent;
            return gameObject;
        }


        public static EffectInfo CreateEffect(EffectInfo effect, VehicleEffectWrapper.VehicleEffectParams desiredParams)
        {
            if(gameObject == null)
            {
                Debug.LogError("Tried to create EffectInfo for " + effectName + " but GameObject was not created!");
                return null;
            }

            var effectWrapper = gameObject.AddComponent<VehicleEffectWrapper>();

            if(effect is LightEffect)
            {
                var lightEffect = effect as LightEffect;

                // There are some cases in which the effect won't render, so we need a copy that can be rendered the way we need it
                if(lightEffect.m_batchedLight || lightEffect.m_positionIndex >= 0)
                {
                    var effect2 = GetModifiedEffect(lightEffect.name) as LightEffect;
                    if(effect2 == null)
                    {
                        GameObject lightObject = new GameObject(lightEffect.name + " - Modified");
                        lightObject.transform.parent = gameObject.transform;

                        var templateLight = lightEffect.GetComponent<Light>();
                        var light = lightObject.AddComponent<Light>();
                        Util.CopyLight(templateLight, light);
                        effect2 = Util.CopyLightEffect(lightEffect, lightObject.AddComponent<LightEffect>());
                        effect2.m_batchedLight = false;
                        effect2.m_positionIndex = -1;
                        effect2.m_position = Vector3.zero;

                        modifiedEffects.Add(lightEffect.name, effect2);
                    }
                    effect = effect2;
                }
            }
            

            effectWrapper.m_wrappedEffect = effect;
            effectWrapper.m_params = desiredParams;

            return effectWrapper;
        }

        public static void OnLevelUnloading()
        {
            modifiedEffects.Clear();
        }

        private static EffectInfo GetModifiedEffect(string effectName)
        {
            EffectInfo result;
            modifiedEffects.TryGetValue(effectName, out result);
            return result;
        }
    }
}
