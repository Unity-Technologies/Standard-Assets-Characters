/**
 *	Set new ProBuilder objects to use special UV2 unwrap params.
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
public class SetUnwrapParams : Editor
{
	/**
	 *	Static constructor is called and subscribes to the OnProBuilderObjectCreated delegate. 
	 */
	static SetUnwrapParams()
	{
		pb_EditorUtility.AddOnObjectCreatedListener(OnProBuilderObjectCreated);
	}

	~SetUnwrapParams()
	{
		pb_EditorUtility.RemoveOnObjectCreatedListener(OnProBuilderObjectCreated);
	}

	/**
	 *	When a new object is created this function is called with a reference to the pb_Object
	 *	last built.
	 */
	static void OnProBuilderObjectCreated(pb_Object pb)
	{
		pb_UnwrapParameters up = pb.unwrapParameters;
		up.hardAngle = 88f;			// range: 1f, 180f
		up.packMargin = 15f;		// range: 1f, 64f
		up.angleError = 30f;		// range: 1f, 75f
		up.areaError = 15f;			// range: 1f, 75f
	}
}

#endif
