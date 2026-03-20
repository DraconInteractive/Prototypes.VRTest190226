using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DI_Sequences
{
    [Serializable]
    public class Sequence
    {
        public enum ActionType
        {
            SelectType,
            Debug,
            MoveTransform,
            ScaleTransform,
            WaitDuration,
            TransferStage,
            UnityEvent
        }
        
        [SerializeReference] public List<SequenceAction> actions = new List<SequenceAction>();

        public bool Running
        {
            get
            {
                foreach (var action in actions)
                {
                    if (action.running)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
    [Serializable]
    public class SequenceAction
    {
        [HideInInspector]
        public string name;
        public bool running;

        public virtual void Begin()
        {
            running = true;
            Run();
        }

        public virtual void Run ()
        {
            Complete();
        }

        public virtual void Complete ()
        {
            running = false;
        }
    }

    [Serializable]
    public class DebugAction : SequenceAction
    {
        public string message;

        public DebugAction()
        {
            name = "Debug";
        }

        public override void Run()
        {
            Debug.Log(message);
            Complete();
        }
    }

    [Serializable]
    public class MoveTransformAction : SequenceAction
    {
        public Transform target;
        public Transform endPosition;
        public float duration;
        public bool instant;

        public MoveTransformAction()
        {
            name = "Move Transform";
        }

        public override void Run()
        {
            if (instant)
            {
                target.position = endPosition.position;
                Complete();
            }
            else
            {
                SequenceManager.All[0].StartCoroutine(Move());
            }
        }

        public IEnumerator Move ()
        {
            Vector3 start = target.position;
            for (float f = 0; f < 1; f += Time.deltaTime / duration)
            {
                target.position = Vector3.Lerp(start, endPosition.position, f);
                yield return null;
            }
            target.position = endPosition.position;
            Complete();
            yield break;
        }
    }

    [Serializable]
    public class ScaleTransformAction : SequenceAction
    {
        public Transform target;
        public Vector3 newScale;
        public float duration;
        public bool instant;

        public ScaleTransformAction ()
        {
            name = "Scale Transform";
        }

        public override void Run()
        {
            if (instant)
            {
                target.localScale = newScale;
                Complete();
            }
            else
            {
                SequenceManager.All[0].StartCoroutine(Scale());
            }
        }

        public IEnumerator Scale ()
        {
            Vector3 start = target.localScale;
            for (float f = 0; f < 1; f += Time.deltaTime / duration)
            {
                target.localScale = Vector3.Lerp(start, newScale, f);
                yield return null;
            }
            target.localScale = newScale;
            Complete();
            yield break;
        }
    }

    [Serializable]
    public class WaitDurationAction : SequenceAction
    {
        public float duration;
        public WaitDurationAction()
        {
            name = "Wait Duration";
        }

        public override void Run()
        {
            SequenceManager.All[0].StartCoroutine(Wait());
        }

        public IEnumerator Wait()
        {
            yield return new WaitForSeconds(duration);
            Complete();
            yield break;
        }
    }

    [Serializable]
    public class TransferStageAction : SequenceAction
    {
        public SequenceManager oldStage;
        public SequenceManager newStage;

        public TransferStageAction ()
        {
            name = "Transfer Stage";
        }

        public override void Run()
        {
            oldStage.StopSequence();
            newStage.StartStage();
            Complete();
        }
    }

    [Serializable]
    public class UnityEventAction : SequenceAction
    {
        public UnityEvent uEvent;

        public UnityEventAction ()
        {
            name = "Unity Event";
        }

        public override void Run()
        {
            uEvent?.Invoke();
            Complete();
        }
    }
    
    [Serializable]
    public class ToggleGameObjectAction : SequenceAction
    {
        public enum Mode {
            Toggle,
            ToggleOn,
            ToggleOff
        }
        
        public Mode mode;
        public GameObject target;
        
        public ToggleGameObjectAction () {
            name = "Toggle GameObject";
        }
        
        public override void Run () {
            switch (mode)
            {
                case Mode.Toggle:
                    target.SetActive(!target.activeSelf);
                    break;
                case Mode.ToggleOn:
                    target.SetActive(true);
                    break;
                case Mode.ToggleOff:
                    target.SetActive(false);
                    break;
            }
            Complete();
        }
    }
    
    [Serializable]
    public class PlayAnimatorStateAction : SequenceAction
    {
        public Animator _animator;
        public string _state;
        
        public PlayAnimatorStateAction () {
            name = "Play Animator State";
        }
        
        public override void Run () {
            _animator.Play(_state);
        }
    }

    [Serializable]
    public class SetAnimatorTriggerAction : SequenceAction
    {
        public Animator _animator;
        public string _trigger;

        public SetAnimatorTriggerAction ()
        {
            name = "Set Animator Trigger";
        }

        public override void Run()
        {
            _animator.SetTrigger(_trigger);
            Complete();
        }
    }

    [Serializable]
    public class FadeInAudioAction : SequenceAction
    {
        public AudioSource _source;
        public float duration;
        public float targetVolume;

        public bool playOnStart;

        public FadeInAudioAction ()
        {
            name = "Fade In Audio";
        }

        public override void Run()
        {
            SequenceManager.All[0].StartCoroutine(RunRoutine());
            if (playOnStart)
            {
                _source.Play();
            }
        }

        IEnumerator RunRoutine ()
        {
            for (float f = 0; f < 1; f += Time.deltaTime / duration)
            {
                _source.volume = Mathf.Lerp(0, targetVolume, f);
                yield return null;
            }
            Complete();
            yield break;
        }
    }
}
