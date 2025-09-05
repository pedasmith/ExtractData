# Simple Exporter for Bing Maps Collections

This app lets you export your Bing Maps Collections in a variety of different file formats.

## Getting Started

The basic workflow is 

1. In the Bing Maps area, be sure to log in with your credentials
2. In the Bing Maps ribbon, click "My Places"
3. In the My Places pane, click "Collections"
4. In the Collection pane, select the collection you want to export
5. From the menu select File > Export and pick the type of export you want.

You might first be told that some markers don't have a latitude and longitude, but the app will attempt to fgure them out. This can take a while, and the map in the app will shake around while it's being found. After a while, the app will report back on how many marker locations could be determined.

You will be prompted for a place to save your files. Pick one, and a file name, and press OK. The actual export is almost instantanous.

## Export types

You can export your collection in different formats.
* Comma separated values (CSV) file which should be compatible with Google Maps
* GeoJSON (RFC 7946) which has been tested with some mapping programs
* HTML can be read using all web browsers and can be copy-pasted into document programs like Word or OneNote
* Markdown is a text file format used for taking notes

It'sm important to know that the exports are just the latitude, longitude, the name of each marker and all of your notes. The output may also include the original raw data straight from the Bing Map collection.


## Can I edit and re-import?

Sorry, no. I don't know of any way to re-import the collection once you export it. But good news, the collection isn't changed by be exported, so you can still edit it in Bing Maps Collections.

## How does the program work?

The program contains an embedded web browser that starts off at Bing Maps. From you you can move the map around and click on user interface. When you have selected a collection, and select File > Export, the app can extract all of the HTML that comprises the web page. In there, somewhere, is the actual collection list in a JSON-style format which the program can then parse.

Some map collection items might have been added from the Bing Maps search. These items don't have a latitude and longitude, which is where the app then gets a little dicey. They don't have a latitude and longitude, but they do have a *ypid* value. Only Bing Maps knows what each ypid means. But we can ask Bing Maps to go to that spot, and when it does, Bing Maps will (eventually) return back a latitude and longitude in the web page's URL bar. The program waits about 5 seconds for this settling to happen.

## Helpful links

There's a quick [YouTube](https://youtu.be/pH2Oap7tSK4) showing how to use the program
The app is available in the [Microsoft Store](https://apps.microsoft.com/detail/9pp7sj1cpl3p)
There's a [Blog Post](https://shipwrecksoftware.wordpress.com/2025/09/04/new-app-simple-exporter-for-bing-maps-collections/)
The app is announced on [LinkedIn](https://www.linkedin.com/posts/peter-smith-4841a0_simple-exporter-for-bing-maps-collections-activity-7369588910407999488-C9HX?utm_source=share&utm_medium=member_desktop&rcm=ACoAAAAG-KQBCfiOiWjuM9Z07Ic7a4-f-khOwNY)
App repo on [GitHub/pedasmith/ExtractData](https://github.com/pedasmith/ExtractData)
The original GitHub [Python](https://github.com/stevendirven1/Bing_Maps_Collections_Export_Python): 

Microsoft [QA](https://learn.microsoft.com/en-us/answers/questions/2275578/how-do-i-export-my-places-from-bing-maps)

