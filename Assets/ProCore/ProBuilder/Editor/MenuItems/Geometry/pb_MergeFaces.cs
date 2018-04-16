#if !PROTOTYPE

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.Actions
{

	/**
	 * Merge 2 or more faces into a single face.  Merged face
	 * retains the properties of the first selected face in the
	 * event of conflicts.
	 *
	 * Deprecated as of 2.6.0.
	 * This file remains only for backwards compatibility; you may
	 * safely delete it.
	 */
	public class pb_MergeFaces : Editor {}
}

#endif
