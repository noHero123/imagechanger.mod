Hy guys,
with this little mod you can simply change the image of your scrolls (NOT the ingame animations).

Here is the source: github link yay

Here is the compiled dll: github link yay

how to use:
create a new folder in  .../Mojang/Scrolls/game/  with the wonderful name:"ownimages".
in there (.../Mojang/Scrolls/game/ownimages/) you place the pictures (.png only) you want to see, instead
of the orginal ones.
Each card has 2 kind of pictures (which could be different) a "big" high resolution Texture
(the Orginal pictures are 300 x 225) which is shown on the specific card infight an on the
big ones in the deckbuilder and store.
and there's a smaller version of the texture (dont know the orginal measurements) which
is only shown on the preview cards in the deckbuilder, the little images in the sell-list and maybe (dont tested it)
in the trading room.
The high-res pictures has to be named after the card, which should be replaced:
if you dont like the orginal "Bear Pawn" image, you name the replacement-picture "Bear Pawn.png".

The low-res pictures has the addition "_prev" after the cardname: to replace the Bear Pawn preview, you
have to name the picture "Bear Pawn_prev.png".

The size of both pictures doesnt matter, they will be scaled up or down to fit on the card, (but
for performance, the _prev.png shouldnt be to big).

You dont have to replace both types of pictures, but I implemented those rules (can easily change it if you want): 
- if you have replaced the high-res picture, but not the low-res one, then the preview-cards take the high-res picture
instead of the orginal-preview image.
- if you replaced only the low-res picture, and not the high-res image of that card, then the orginal high-res image
is shown.
(the high res and low res pictures dont need to have the same motive :D)

have fun
noHero