#Welcome to the parser, now remember, you're here forever
##First things first
Websites are trees.
The Html tag has the children: header, body and footer and so on. This is the way that the parser sees websites.
##Searching
In the HTMLImporter class we have a static method called "RunWithSearch" that is used to complete searches.
This function returns a list of list of dataholders, a list of sites which are made up of a list of dataholders.
A dataholder is just that, a class that is temporary and holds the data for later use. the textdataholder has a title and text field
but most things only have a title and uri.
With the search function we just pass in our search string and it asyncronously parses through all of the websites. Handy!
We don't need to instantiate an HTMLImporter because that would be unnecessary.
##Parsing
The actual recursive method that parses looks dense, but it's pretty simple.
We start by finding an article or mw-body tag and then begin the parse from there
We have a list of bad html tags/classes/ids and we skip those tags until we get to the actual data that we want to find
We then look at different tags to find if we run into text, images, pdfs, etc. and then we add that data into a dataholder and then
continue our parse
##What next?
Well most of the tweaking that can be done is with optimizations with the async code that is in the search function or in the 
regexes(haha hope you know regex) in the parser that remove and include different things to be parsed