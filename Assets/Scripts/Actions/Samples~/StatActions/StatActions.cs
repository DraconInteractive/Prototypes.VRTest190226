using System;
using System.Collections.Generic;
using System.Text;
#if di_stats
using DI_Statistics;
#endif
namespace DI_Sequences
{
#if di_stats

    [Serializable]
    public class AddHealthAction : SequenceAction
    {
        public CharacterStats _stats;
        public float _amount;

        public AddHealthAction () {
            name = "Add Health";
        }

        public override void Run () {
            _stats.BuffHealth(_amount);
            Complete();
        }
    }
    
#endif
}
