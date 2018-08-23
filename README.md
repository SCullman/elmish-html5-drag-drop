# Elmish HTML5 Drag and Drop Demo
## Towers of Hanoi

This is a direct port of the [Towers of Hanoi](https://en.wikipedia.org/wiki/Tower_of_Hanoi) example [elm-html5-drag-drop](https://github.com/wintvelt/elm-html5-drag-drop) to Fable/Elmish.


## Requirements

* [dotnet SDK](https://www.microsoft.com/net/download/core) 2.0 or higher
* [node.js](https://nodejs.org) with [npm](https://www.npmjs.com/)

## Building and running the app

* Install JS dependencies: `npm install`
* Install F# dependencies: `dotnet restore src`
* Move to `src` directory to start Fable and Webpack dev server: `dotnet fable webpack-dev-server`
* After the first compilation is finished, in your browser open: http://localhost:8080/

Any modification you do to the F# code will be reflected in the web page after saving.

