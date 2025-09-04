# Work list for the Simple Bing Map Collection Extractor

### Big items

Privacy blocks so I can make screen shots with confidential information blocked out. Or remove them all, since I've can use my Shipwreck account?

### Clean up reminders
Add images to the Help text?
Review all help
Check all code TODO: and DBG: and here!here comments. AS of 2025-09-02 19:50, there are none!

### Medium and small items

2025-08-02: DONE: Add Feedback dialog box (will need to have a hyper link for a mailto:) 
2025-09-02: DONE: Remove the tab! Just have Extract be the norm, and then overlay a developer or help as needed. (later: looks so much nicer without the tabs)
2025-09-02: DONE: Update Ypid cache to use the "right" directory. 
2025-09-02: DONE: Added HTML with Simple.css from https://simplecss.org/ (and tipped him!)
2025-09-01: DONE: Menus + Help system + Version check
2025-09-01: DONE: Remove the "debug" button
2025-09-01: DONE: The app icon in the title bar is wrong?

2025-09-01: DONE: Additional output: Markdown. And it saves "all the data"
2025-09-01: DONE: UX for the extract. Normally it's super fast but converting the ypid takes time
2025-09-01: DONE: UX for when there isn't a Collections visible. Gather useful debug info and offer to email?
2025-09-01: DONE: N Items: ___ should be a cheerier message about how the extract is complete
2025-09-01: DONE: Sort output
2025-08-31: DONE: Export. Use a file picker. Uses the collection name for the suggested file name.
2025-08-31: DONE: Icons + added to Gihub
2025-08-30: DONE: The Ypid work queue is wrong! It starts off at "my" location. The only choice is to wait e.g. 5 seconds?
2025-08-30: DONE: Parse the Ypid URL to get the cp latitude and longitude. Save latitude and longitude in MapCollectionItem
2025-08-30: DONE: Add new YPID class. Usage pattern is add to queue, start queue, wait for completion, get results. 
2025-08-28: DONE: Add parser for query=51.507753, -0.121518
2025-08-28: DONE: Automatically load the map
2025-08-28: DONE: Update the URL field to include the redirects. 
2025-08-28: FAILED: Remember the the URLs that have been extracted. But the collections don't have a unique URL
2025-08-28: DONE: Move code to a control away from the Window (so I get a loaded event)
