#load "build/Docs.fsx"

open Partas.Build

exit (Docs.run (Args.script ()))
