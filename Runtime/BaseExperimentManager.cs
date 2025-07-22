using System;
using System.Collections.Generic;
using UnityEngine;

namespace RicKit.Experiment
{
    public interface IExperimentManager
    {
        void InitExperimentManager();
        void RegisterExperiment<T>() where T : BaseExperiment, new();
        void DoExperiment<T>(Action<T> action) where T : BaseExperiment;
        T GetExperiment<T>() where T : BaseExperiment, new();
    }

    public abstract class BaseExperimentManager : IExperimentManager
    {
        protected readonly Dictionary<string, BaseExperiment> experiments = new Dictionary<string, BaseExperiment>();
        private const string FirstTimeLogin = "ExperimentFirstTimeLogin";
        private const string VersionKey = "ExperimentVersion";
        private bool firstTimeThisVersion, firstTimeLogin;
        private bool isInitialized;
        
        public void InitExperimentManager()
        {
            if (isInitialized)
            {
                Debug.LogWarning("ExperimentManager 已经初始化过了");
                return;
            }
            isInitialized = true;
            firstTimeThisVersion = GetString(VersionKey) != Application.version;
            firstTimeLogin = GetInt(FirstTimeLogin) == 0;
            Debug.Log(firstTimeLogin ? "新玩家" : firstTimeThisVersion ? "新版本老玩家" : "不触发分组");
            SetInt(FirstTimeLogin, 1);
            SetString(VersionKey, Application.version);
            Save();
        }
        private bool IsInitialized()
        {
            if (isInitialized) return true;
            Debug.LogError("ExperimentManager 未初始化，请先调用 InitExperimentManager()");
            return false;
        }

        private T LoadExperiment<T>() where T : BaseExperiment, new()
        {
            var exp = new T();
            var key = typeof(T).Name;
            var data = GetString(key);
            return !string.IsNullOrEmpty(data) ? exp.FromString(data) as T : exp;
        }

        private void SaveExperiment<T>(T exp) where T : BaseExperiment
        {
            if (!IsInitialized()) return;
            var key = exp.GetType().Name;
            SetString(key, exp.ToString());
            Save();
        }

        public void RegisterExperiment<T>() where T : BaseExperiment, new()
        {
            if (!IsInitialized()) return;
            var exp = LoadExperiment<T>();
            experiments[typeof(T).Name] = exp;
            switch (exp.TargetUser)
            {
                case ExperimentTargetUser.None:
                    break;
                case ExperimentTargetUser.NewUser:
                    if (firstTimeLogin)
                    {
                        exp.SetGroup();
                        SaveExperiment(exp);
                    }

                    break;
                case ExperimentTargetUser.OldUser:
                    if (!firstTimeLogin && firstTimeThisVersion)
                    {
                        exp.SetGroup();
                        SaveExperiment(exp);
                    }

                    break;
            }

            Debug.Log($"{typeof(T).Name}\n{exp}\n是否为实验目标：{exp.group != ExperimentGroup.None}\n分组：{exp.group}");
            SaveExperiment(exp);
        }

        public void DoExperiment<T>(Action<T> action) where T : BaseExperiment
        {
            if (!IsInitialized()) return;
            var exp = experiments[typeof(T).Name];
            action(exp as T);
        }

        public T GetExperiment<T>() where T : BaseExperiment, new()
        {
            if (!IsInitialized()) return null;
            return experiments[typeof(T).Name] as T;
        }

        #region Prefs

        protected abstract void SetInt(string key, int value);
        protected abstract void SetString(string key, string value);
        protected abstract int GetInt(string key);
        protected abstract string GetString(string key);
        protected abstract void Save();

        #endregion
    }
}