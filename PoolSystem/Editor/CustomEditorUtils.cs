using UnityEngine;

public static class CustomEditorUtils
{
	public static Texture2D MakeTex(Color col)
	{
		Color[] pix = new Color[1 * 1];
		for (int i = 0; i < pix.Length; i++)
			pix[i] = col;
		Texture2D result = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		result.hideFlags = HideFlags.HideAndDontSave;
		result.SetPixels(pix);
		result.Apply();
		return result;
	}
}