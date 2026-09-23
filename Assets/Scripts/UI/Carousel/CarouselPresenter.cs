using System;
using R3;
namespace UI.Carousel
{
	public class CarouselPresenter<T> : IDisposable
	{
		private readonly CarouselModel<T> _model;
		private readonly CarouselView<T> _view;
		private IDisposable _subscription;

		public CarouselPresenter(CarouselModel<T> model, CarouselView<T> view)
		{
			_model = model;
			_view = view;
		}

		public void Initialize()
		{
			_subscription = Disposable.Combine(_model.CurrentItem.Subscribe(_view.OnModelValueChanged),
				_view.IncreaseButtonPressed.Subscribe(_ => _model.MoveForward()),
				_view.DecreaseButtonPressed.Subscribe(_ => _model.MoveBack()));
		}

		public void Dispose()
		{
			_subscription?.Dispose();
		}
	}
}