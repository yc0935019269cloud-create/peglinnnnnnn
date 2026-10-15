using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class OneShotAnimation : MonoBehaviour
{
	public delegate void AnimationEnded();

	public AnimationClip Clip;

	protected PlayableGraph _graph;

	protected AnimationClipPlayable _clipPlayable;

	public AnimationEnded OnAnimationEnded;

	protected bool shouldPlay;

	public void Play()
	{
		shouldPlay = true;
		_graph = PlayableGraph.Create();
		_graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
		AnimationPlayableOutput output = AnimationPlayableOutput.Create(_graph, "Animation", GetComponent<Animator>());
		_clipPlayable = AnimationClipPlayable.Create(_graph, Clip);
		output.SetSourcePlayable(_clipPlayable);
		_graph.Play();
	}

	private void Update()
	{
		if (shouldPlay && _clipPlayable.IsValid() && _clipPlayable.GetTime() >= (double)Clip.length)
		{
			shouldPlay = false;
			OnAnimationEnded?.Invoke();
			if (_graph.IsValid())
			{
				_graph.Destroy();
			}
		}
	}

	private void OnDisable()
	{
		shouldPlay = false;
		if (_graph.IsValid())
		{
			_graph.Destroy();
		}
	}
}
