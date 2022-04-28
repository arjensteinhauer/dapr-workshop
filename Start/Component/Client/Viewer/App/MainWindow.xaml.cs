namespace Game.Client.Viewer.App
{
    using Dapr.Actors;
    using Dapr.Actors.Client;
    using Dapr.Client;
    using Game.Actor.Shape.Interface;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string MembershipAppId = "membership";
        private static bool isClosing = false;
        private readonly IShapeActorEventsHandler _shapeActorEventsHandler;
        private readonly ConcurrentDictionary<Guid, ShapeState> _shapes = new ConcurrentDictionary<Guid, ShapeState>();

        /// <summary>
        /// Gets the client ID from the app settings.
        /// </summary>
        private Guid ClientId => Guid.Parse(ConfigurationManager.AppSettings["clientId"]);


        /// <summary>
        /// Shape state.
        /// </summary>
        private class ShapeState
        {
            private readonly Canvas _shapesCanvas;

            /// <summary>
            /// The shape actor.
            /// </summary>
            public IShapeActor ShapeActor { get; private set; }

            /// <summary>
            /// The shape control.
            /// </summary>
            public Rectangle ShapeControl { get; private set; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="shapeActor">The shape actor.</param>
            public ShapeState(IShapeActor shapeActor, Rectangle shapeControl, Canvas shapesCanvas)
            {
                ShapeActor = shapeActor;
                ShapeControl = shapeControl;
                _shapesCanvas = shapesCanvas;
            }

            /// <summary>
            /// Update the shape to it's latest location.
            /// </summary>
            public async Task UpdateToLatestLocation()
            {
                if (isClosing)
                {
                    return;
                }

                try
                {
                    var currentShapeLocation = await ShapeActor.RetrievePosition();

                    await App.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ShapeControl.Stroke = new SolidColorBrush(Colors.Yellow);
                        Canvas.SetTop(ShapeControl, currentShapeLocation.Y);
                        Canvas.SetLeft(ShapeControl, currentShapeLocation.X);
                        ((RotateTransform)ShapeControl.RenderTransform).Angle = currentShapeLocation.Angle;

                        if (!_shapesCanvas.Children.Contains(ShapeControl))
                        {
                            _shapesCanvas.Children.Add(ShapeControl);
                        }
                    });
                }
                catch
                {
                    // error on actor
                    ShapeControl.Stroke = new SolidColorBrush(Colors.Red);
                }
            }
        }

        public MainWindow(IShapeActorEventsHandler shapeActorEventsHandler)
        {
            _shapeActorEventsHandler = shapeActorEventsHandler;
            _shapeActorEventsHandler.OnUpdatedShapeLocation += async (sender, shapeId) => await ShapeActorEventsHandler_OnUpdatedShapeLocation(shapeId);

            InitializeComponent();
        }


        /// <summary>
        /// Event called before closing the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;

            // unsubscibe from SignalR
            await _shapeActorEventsHandler.Disconnect().ConfigureAwait(false);
        }

        /// <summary>
        /// Event called after the window has loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // subscibe on shape events from SignalR
                await _shapeActorEventsHandler.Connect().ConfigureAwait(false);

                // onboard the shape viewer app and retrieve the list of active shapes for this client (active shape actors)

               /*---------------------------------------------------------------------------------------
                * EXCERCISE 6: use the DAPR client service invocation to onboard the shape viewer app instance
                *---------------------------------------------------------------------------------------
                var shapelist = ...

                // restore the shapes in the UI
                await Task.WhenAll(shapeList.Select(shapeId => CreateShape(shapeId)));

                *---------------------------------------------------------------------------------------*/
            }
            catch (Exception ex)
            {
                string errorText = GetExceptionMessageText(ex);
                MessageBox.Show(errorText);
            }
        }

        /// <summary>
        /// AddShapeButton click event. Add a new shape to the canvas.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event paraeters.</param>
        private async void AddShapeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // create a new shape ID
                Guid shapeId = Guid.NewGuid();

                // create a new shape
                await CreateShape(shapeId);
            }
            catch (Exception ex)
            {
                string errorText = GetExceptionMessageText(ex);
                MessageBox.Show(errorText);
            }
        }

        private async Task ShapeActorEventsHandler_OnUpdatedShapeLocation(Guid shapeId)
        {
            if (_shapes.ContainsKey(shapeId))
            {
                // refresh the shape location on the canvas
                await _shapes[shapeId].UpdateToLatestLocation();
            }
        }

        /// <summary>
        /// Creates a shape and adds it to the canvas.
        /// </summary>
        /// <param name="shapeId">The ID of the shape to add.</param>
        /// <returns>Async task.</returns>
        private async Task CreateShape(Guid shapeId)
        {
            // create a shape UI element
            Rectangle shapeControl = null;
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                RotateTransform rotation = new RotateTransform() { Angle = 0, CenterX = 50, CenterY = 50 };
                shapeControl = new Rectangle()
                {
                    Height = 100,
                    Width = 100,
                    Stroke = new SolidColorBrush(Colors.Yellow),
                    StrokeThickness = 5,
                    RadiusX = 20,
                    RadiusY = 20,
                    RenderTransform = rotation
                };
            });

            // create a new shape actor
            ActorId actorId = new ActorId($"{ClientId:N}_{shapeId:N}");
            IShapeActor shapeActor = ActorProxy.Create<IShapeActor>(actorId, "ShapeActor");

            // save the shape state
            var shapeState = new ShapeState(shapeActor, shapeControl, ShapesCanvas);
            _shapes.TryAdd(shapeId, shapeState);
            await shapeState.UpdateToLatestLocation();
        }

        /// <summary>
        /// Gets the error message from the provided exception.
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <returns>The error message.</returns>
        private static string GetExceptionMessageText(Exception ex)
        {
            string errorMessage = ex.Message;
            if (ex.InnerException != null)
            {
                errorMessage += string.Format("\r\nInner exception:\r\n{0}", GetExceptionMessageText(ex.InnerException));
            }

            return errorMessage;
        }
    }
}
