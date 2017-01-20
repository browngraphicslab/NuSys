﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public class ContentController
    {
        /// <summary>
        /// returns te static class's private instance
        /// </summary>
        public SQLConnector SqlConnector
        {
            get { return _sqlConnector; }
        }

        /// <summary>
        /// the comparison controller for the server, acts as a static class
        /// </summary>
        public ComparisonController ComparisonController
        {
            get
            {
                return _comparisonController;
            }
        }

        private SQLConnector _sqlConnector;
        private ComparisonController _comparisonController;
        private static ContentController _instance = null;
        public static ContentController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ContentController();
                }
                return _instance;
            }
        }

        /// <summary>
        /// private constructor to make singleton
        /// </summary>
        private ContentController()
        {
            _sqlConnector = new SQLConnector();
            _comparisonController = new ComparisonController();
        }
    }
}