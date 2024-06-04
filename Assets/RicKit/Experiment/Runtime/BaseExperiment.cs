
namespace RicKit.Experiment
{
    public enum ExperimentGroup
    {
        None,
        S,
        A,
        B
    }

    public enum ExperimentTargetUser
    {
        None,
        NewUser,
        OldUser
    }
    public abstract class BaseExperiment
    {
        public ExperimentGroup group;
        public abstract ExperimentTargetUser TargetUser { get; }
        public bool isTarget;
        public abstract override string ToString();
        public abstract BaseExperiment FromString(string data);
        /// <summary>
        /// 当用户为实验目标时首次进入游戏调用，用于分组
        /// </summary>
        public abstract void SetGroup();
    }
}