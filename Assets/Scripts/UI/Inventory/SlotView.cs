using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GamePlay;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
	public sealed class SlotView : MonoBehaviour
	{
		[SerializeField] private RectTransform uiArea;
		
		public IEnumerable<MeshRenderer> MeshRenderers => _meshRenderers;
		private readonly List<MeshRenderer> _meshRenderers = new List<MeshRenderer>();
		
		public void Select()
		{
			uiArea.localScale *= 1.5f;
		}

		public void Deselect()
		{
			uiArea.localScale /= 1.5f;	
		}

		public void AddModel(InventoryItem item)
		{
			if (item.MeshFilters.Length == 0)
			{
				return;
			}
			
			Canvas.ForceUpdateCanvases();
			
			float uiWorldSize = Mathf.Min(uiArea.rect.width, uiArea.rect.height);
			Bounds modelBounds = item.MeshFilters.First().mesh.bounds;
			
			foreach (MeshFilter meshFilter in item.MeshFilters)
			{
				GameObject go = meshFilter.gameObject;
				GameObject modelCopy = Instantiate(go, transform);
				modelCopy.name = go.name;
				modelCopy.layer = LayerMask.NameToLayer("UI");
				modelCopy.transform.DORotate(new Vector3(0, 360, 0), 5f, RotateMode.FastBeyond360)
					.SetRelative(true)
					.SetEase(Ease.Linear)
					.SetLoops(-1)
					.SetLink(modelCopy);
				
				modelBounds.Encapsulate(meshFilter.mesh.bounds);
				
				var meshRenderer = modelCopy.GetComponent<MeshRenderer>();
				meshRenderer.enabled = true;
				_meshRenderers.Add(meshRenderer);
			}
			
			float targetScale = uiWorldSize / Mathf.Max(modelBounds.size.x, modelBounds.size.y, modelBounds.size.z);

			foreach (MeshRenderer meshRenderer in _meshRenderers)
			{
				meshRenderer.transform.localScale = Vector3.one * targetScale;
			}
		}

		public void RemoveModel()
		{
			foreach (var meshRender in _meshRenderers)
			{
				Destroy(meshRender.gameObject);
			}
			
			_meshRenderers.Clear();
		}
	}
}