using UnityEngine;
using System.Collections;
using System.Text;

public class Util  {
	
#if UNITY_ANDROID
	static public string AStringToCString(AndroidJavaObject astring){
		char[] cArrayForFre = astring.Call<char[]> ("toCharArray");
		return new string (cArrayForFre);
	}
#endif
	
	static public int transferString2IntFrom(string str, ref int searchIndex)
	{
		char[] cArray = str.ToCharArray ();
		StringBuilder num = new StringBuilder();
		for (int i=searchIndex; i < str.Length; i++) {
			char c = cArray[i];
			if (c >= '0' && c <= '9')
			{
				num.Append(c);
			}
			else{
				if (num.Length != 0)
				{
					break;
				}
			}
			
			searchIndex ++;
		}
		
		if (num.Length != 0) {
			return System.Int32.Parse (num.ToString());
		}
		
		return 0;
	}

	static public double transferString2DoubleFrom(string str, int searchIndex)
	{
		bool containsDotAlready = false;
		char[] cArray = str.ToCharArray ();
		StringBuilder num = new StringBuilder();
		for (int i=searchIndex; i < str.Length; i++) {
			char c = cArray[i];
			if (c >= '0' && c <= '9')
			{
				num.Append(c);
			}
			else if (c == '.'){
				if (!containsDotAlready){
					num.Append(c);
					containsDotAlready = true;
				}
				else
				{
					break;
				}
			}
			else{
				if (num.Length != 0)
				{
					break;
				}
			}
			
			searchIndex ++;
		}
		
		if (num.Length != 0) {

			return System.Double.Parse (num.ToString());
		}
		
		return 0;
	}

}
