# Work list for the Simple Bing Map Collection Extractor

### Big items

Menus + Help system + Version check
Privacy blocks so I can make screen shots with confidential information blocked out
Output to format?
Icons

### Clean up reminders
Remove the "debug" button

### Convert ypid and sid

Update Ypid cache to use the "right" directory
UX for the extract. Normally it's super fast but converting the ypid takes time
UX for when there isn't a Collections visible. Gather useful debug info and offer to email?
N Items: ___ should be a cheerier message about how the extract is complete
Use the collection name for the output?


2025-08-30: DONE: The Ypid work queue is wrong! It starts off at "my" location. The only choice is to wait e.g. 5 seconds?
2025-08-30: DONE: Parse the Ypid URL to get the cp latitude and longitude. Save latitude and longitude in MapCollectionItem
2025-08-30: DONE: Add new YPID class. Usage pattern is add to queue, start queue, wait for completion, get results. 
2025-08-28: DONE: Add parser for query=51.507753, -0.121518
2025-08-28: DONE: Automatically load the map
2025-08-28: DONE: Update the URL field to include the redirects. 
2025-08-28: FAILED: Remember the the URLs that have been extracted. But the collections don't have a unique URL
2025-08-28: DONE: Move code to a control away from the Window (so I get a loaded event)
