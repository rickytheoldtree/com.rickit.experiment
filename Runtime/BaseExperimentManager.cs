using System;
using System.Collections.Generic;
using UnityEngine;

namespace RicKit.Experiment
{
    public interface IPrefs
    {
    }

    public abstract class BaseExperimentManager
    {
        private const string FirstTimeLogin = "ExperimentFirstTimeLogin";
        private const string VersionKey = "ExperimentVersion";
        protected readonly Dictionary<string, BaseExperiment> experiments = new Dictionary<string, BaseExperiment>();

        /// <summary>
        /// 在注册完所有实验后调用
        /// </summary>
        public void Init()
        {
            SetGroup(GetString(VersionKey) != Application.version,
                GetInt(FirstTimeLogin) == 0);
            SetInt(FirstTimeLogin, 1);
            SetString(VersionKey, Application.version);
            Save();
        }

        private void SetGroup(bool firstTimeThisVersion, bool firstTimeLogin)
        {
            Debug.Log(firstTimeLogin ? "新玩家" : firstTimeThisVersion ? "新版本老玩家" : "不触发分组");
            foreach (var experiment in experiments)
            {
                var exp = experiment.Value;
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

                Debug.Log($"{experiment.Key}\n{exp}\n是否为实验目标：{exp.isTarget}\n分组：{exp.group}");
            }
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
            var key = exp.GetType().Name;
            SetString(key, exp.ToString());
            Save();
        }

        protected void RegisterExperiment<T>() where T : BaseExperiment, new()
        {
            var exp = LoadExperiment<T>();
            experiments[typeof(T).Name] = exp;
        }

        public void DoExperiment<T>(Action<T> action) where T : BaseExperiment
        {
            var exp = experiments[typeof(T).Name];
            action(exp as T);
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