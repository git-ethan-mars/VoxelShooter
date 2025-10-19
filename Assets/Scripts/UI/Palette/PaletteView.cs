using System.Threading;
using Cysharp.Threading.Tasks;
namespace UI
{
	public class PaletteView : ListView<PaletteElementView>
	{
		private CancellationTokenSource _cts;
		private PaletteElementView _selectedElement;

		public async UniTask SelectElementAsync(int index)
		{
			if (_selectedElement != null)
			{
				_cts.Cancel();
				_cts.Dispose();
			}

			_cts = new CancellationTokenSource();
			_selectedElement = Items[index];
			await _selectedElement.RunAnimationAsync(_cts.Token);
		}
	}
}