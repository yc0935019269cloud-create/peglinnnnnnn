using UnityEngine;

namespace Loading.Pegboards
{
	public interface IPostProcessAction
	{
		void DoPostProcessAction(Transform root);
	}
}
