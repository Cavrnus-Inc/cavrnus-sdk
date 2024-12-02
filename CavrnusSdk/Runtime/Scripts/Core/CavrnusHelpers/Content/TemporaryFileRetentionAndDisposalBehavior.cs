using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collab.Base.Core;
using Collab.Base.Math;
using UnityEngine;

namespace UnityBase.Content
{
	public class TemporaryFileRetentionAndDisposalBehavior : MonoBehaviour
	{
		public string filePath = null;
		private bool live = false;

		public void Awake()
		{
			live = true;
			if (filePath != null)
			{
				TemporaryFileRetentionRecord.Retain(filePath);
			}
		}

		public void SetFilePath(string fp)
		{
			if (filePath == fp)
				return; // Don't release/renew if it isn't changing!

			if (filePath != null && live)
			{
				TemporaryFileRetentionRecord.Release(filePath);
			}

			filePath = fp;

			if (filePath != null && live)
			{
				TemporaryFileRetentionRecord.Retain(filePath);
			}
		}

		public void OnDestroy()
		{
			if (filePath == null)
				return;
			live = false;
			TemporaryFileRetentionRecord.Release(filePath);
			filePath = null;
		}
	}

	public static class TemporaryFileRetentionRecord
	{
		private static Dictionary<string, CountedBool> filemap = new Dictionary<string, CountedBool>();

		public static void Retain(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
				return;
			if (filemap.TryGetValue(path, out var count))
			{
				count.Set();
			}
			else
			{
				var c = new CountedBool();
				c.Set();
				filemap.Add(path, c);
			}
		}

		public static void Release(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
				return;
			if (filemap.TryGetValue(path, out var count))
			{
				if (count.Unset())
				{
					try
					{
						File.Delete(path);
					}
					catch (IOException e)
					{
						DebugOutput.Log($"Failed to flush temporary file '{path}'.");
					}

					filemap.Remove(path);
				}
			}
		}

	}
}
