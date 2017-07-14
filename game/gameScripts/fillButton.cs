function fillButton::onLevelLoaded(%this, %scenegraph)
{
   %this.useMouseEvents = true;
}

function fillButton::onMouseDown(%this, %modifier, %worldPosition, %mouseClicks)
{
   echo("Player" SPC %this.Player SPC "clicked on color: " @ %this.getImageMap());
   
   //remove the mouse interaction - prevents a crash when double clicking a button
   %this.useMouseEvents = false;
   
   mainGrid.setCurrentColor(%this.Player, %this.getImageMap());
   
   //re-enable the mouse
   %this.schedule(500, setUseMouseEvents, true);
   
}