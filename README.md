# GiggleTokenWeb
A giggle token generator in .NET 8 AOT. 
Computes tokens with only ~300B allocated and 500ns per generation. 

You can grab the docker image at `carbonneuron/giggletokenweb:latest`

Compute tokens at 
http://`{IP}`:8080/tokens

provide a GUID to compute for, or a number to generate, and optionally provide a length (defaults to 156)

http://`{IP}`:8080/tokens/245CFD0D-307B-4164-82EC-1AFB1E592F68?length=112

http://`{IP}`:8080/tokens/100?length=112

If you request application/json, you will get a list back of the IDs, if you don't you'll get them delimited by `\n`
