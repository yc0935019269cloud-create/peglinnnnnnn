namespace PixelCrushers
{
	public static class SceneNotifier
	{
		public delegate void UnloadSceneDelegate(int sceneIndex);

		public static event UnloadSceneDelegate willUnloadScene = delegate
		{
		};

		public static void NotifyWillUnloadScene(int sceneIndex)
		{
			willUnloadScene(sceneIndex);
		}
	}
}
