/**
 *	This script demonstrates how one might use the OnproBuilderObjectCreated delegate.
 */

// Uncomment this line to enable this script.
// #define PROBUILDER_API_EXAMPLE

#if PROBUILDER_API_EXAMPLE

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

[InitializeOnLoad]
public class RenameNewObjects : Editor
{
	/**
	 *	Static constructor is called and subscribes to the OnProBuilderObjectCreated delegate. 
	 */
	static RenameNewObjects()
	{
		pb_EditorUtility.AddOnObjectCreatedListener(OnProBuilderObjectCreated);
	}

	~RenameNewObjects()
	{
		pb_EditorUtility.RemoveOnObjectCreatedListener(OnProBuilderObjectCreated);
	}

	/**
	 *	When a new object is created this function is called with a reference to the pb_Object
	 *	last built.
	 */
	static void OnProBuilderObjectCreated(pb_Object pb)
	{
		pb.gameObject.name = string.Format("pb_{0}{1}", pb.gameObject.name, pb.GetInstanceID());
	}
}

#endif
