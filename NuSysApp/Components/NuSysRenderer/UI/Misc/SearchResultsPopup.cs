using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// this is a pop up that will give users options of what they would like to do with their bing search results.
    /// there are two options - make a collection, and add to collection as a stack.
    /// </summary>
    public class SearchResultsPopup : PopupUIElement
    {
        /// <summary>
        /// a list of all the library element models of the results. 
        /// used to add to a new collection or add the results to the current collection.
        /// </summary>
        private List<LibraryElementController> _results;

        /// <summary>
        /// the search term used to get the web search results
        /// </summary>
        private string _searchTerm;

        /// <summary>
        /// prompt text
        /// </summary>
        private TextboxUIElement _text;

        /// <summary>
        /// button to add the results as a collection to the current collection you are in
        /// </summary>
        private RectangleButtonUIElement _addCollection;

        /// <summary>
        /// button to add the results as a stack to the current collection you are in
        /// </summary>
        private RectangleButtonUIElement _addStack;

        /// <summary>
        /// closes the popup
        /// </summary>
        private RectangleButtonUIElement _closebutton;

        /// <summary>
        /// top padding for elements within popup
        /// </summary>
        private float _topPadding = 20;

        /// <summary>
        /// constructor for search results pop up.
        /// pass in the results from the search and also the search term used. 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public SearchResultsPopup(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, List<LibraryElementController> results, string searchTerm) : base(parent, resourceCreator)
        {
            // set appearance
            Background = Colors.White;
            BorderWidth = 1;
            BorderColor = Constants.DARK_BLUE;
            Height = 300;
            Width = 500;

            // set values of results and search term
            _results = results;
            _searchTerm = searchTerm;

            // set up prompt text box and add it as a child
            _text = new TextboxUIElement(this, resourceCreator)
            {
                Text="What would you like to do with these results?",
                Background = Colors.White,
                FontFamily = UIDefaults.TextFont,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                Width = 250
            };
            AddChild(_text);
            _text.Transform.LocalPosition = new Vector2(Width/2 - _text.Width/2, _topPadding);

            // set up add collection button
            _addCollection = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.PrimaryStyle, "Add As Collection");
            AddChild(_addCollection);
            _addCollection.Transform.LocalPosition = new Vector2(Width/2 - _addCollection.Width/2, _text.Transform.LocalY + _text.Height + _topPadding);

            // set up add as stack button
            _addStack = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.PrimaryStyle, "Add As Stack");
            AddChild(_addStack);
            _addStack.Transform.LocalPosition = new Vector2(Width/2 - _addStack.Width/2, _addCollection.Transform.LocalY + _addCollection.Height + _topPadding);

            //set up close button
            _closebutton = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.PrimaryStyle, "Do Nothing");
            AddChild(_closebutton);
            _closebutton.Transform.LocalPosition = new Vector2(Width/2 - _closebutton.Width/2, _addStack.Transform.LocalY + _addStack.Height + _topPadding);

            // add handlers to the two buttons
            _addCollection.Tapped += AddCollection_Tapped;
            _addStack.Tapped += AddStack_Tapped;
            _closebutton.Tapped += Closebutton_Tapped;

            // set position
            Transform.LocalPosition = new Vector2(SessionController.Instance.NuSessionView.Width / 2 - Width / 2, SessionController.Instance.NuSessionView.Height / 2 - Height / 2);

        }

        /// <summary>
        /// closes the popup without doing anything to the search results
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void Closebutton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            DismissPopup();
        }

        /// <summary>
        /// add the search results to the collection as a stack.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void AddStack_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            StaticServerCalls.CreateStackOnMainCollection(new Vector2(0, 0), _results);
            DismissPopup();
        }

        /// <summary>
        /// add the search results to the collection as a new collection from the search term
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void AddCollection_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            StaticServerCalls.CreateCollectionOnMainCollection(new Vector2((float)Constants.InitialCenter, (float)Constants.InitialCenter), _results,
                _searchTerm);
            DismissPopup();
        }

        /// <summary>
        /// dispose method to get rid of handlers
        /// </summary>
        public override void Dispose()
        {
            _addCollection.Tapped -= AddCollection_Tapped;
            _addStack.Tapped -= AddStack_Tapped;
            _closebutton.Tapped -= Closebutton_Tapped;
        }
    }
}
