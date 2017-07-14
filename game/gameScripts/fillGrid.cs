//TODO: create the grid progmatically so we can adjust the size in options
function fillGrid::onLevelLoaded(%this, %scenegraph)
{
   //echo("fillGrid loaded");
   
   startTimer();
   
   // NOTE 2: WE WANT A GRID EXACTLY LIKE THE ORIGINAL.  SINCE IT IS
   // ADDED AFTER THE ORIGINAL, IT WILL AUTOMATICALLY BE ON TOP.
   %this.borders = %scenegraph.getGlobalTileMap().createTileLayer(
       %this.getTileCount(), %this.getTileSize() );
   %this.borders.setPosition( %this.getPosition() );
   %this.borders.setSize( %this.getSize() );
   %this.borders.setBlendColor( 0.63, 0.19, 0.01 ); // Black lines
   
   %maxX = %this.getTileCountX();
   %maxY = %this.getTileCountX();
   
   for(%x = 0; %x < %maxX; %x++)
   {
      for(%y = 0; %y < %maxY; %y++)
      {       
         %fillTile = %this.getRandomTileColor();
         %this.setStaticTile(%x, %y, %fillTile,0); 
         // NOTE THAT I SET THE PLAYER FIELD TO 0.  IT'S NOT
         // NECESSARY, BUT IT MAKES THE CUSTOM DATA MORE CONSISTENT.
         %this.setTileCustomData(%x, %y, %fillTile SPC 0); 
      }
   }
   
   
   // NOTE: I'VE SET UP THE VARIABLES SO THAT THEY CAN BE ACCESSED
   // BY THE PLAYER NUMBER ALONE.
    
   
   
   
   //set tiles owned 
   %this.playerOwned[1] = 0;
   %this.playerOwnedLabel[1] = player1OwnedLabel;
   
   %this.playerOwned[2] = 0;
   %this.playerOwnedLabel[2] = player2OwnedLabel;
      
   //set starting positions
   %this.start[1] = "0 0";
   %this.start[2] = (%maxX - 1) SPC (%maxY - 1);
   
   //peek and see how many each player randomly owns at the start
   %this.floodOwn( 1 );
   %this.floodOwn( 2 );
   
   //set clicks (having this after the peekr resets the count back to 0
   %this.playerClicks[1] = 0;
   %this.playerClicksLabel[1] = player1ClicksLabel;

   %this.playerClicks[2] = 0;
   %this.playerClicksLabel[2] = player2ClicksLabel;
   
   %this.updateBorders();
   
   //init gui values
   %this.setScores(); 
}

//called when user clicks a color
function fillGrid::setCurrentColor( %this, %player, %newColor )
{  
  %this.playerClicks[%player]++;
  %this.playerClicksLabel[%player].text = %this.playerClicks[%player];
  
  %this.floodFill( %player, %newColor );
  %this.floodOwn( %player );
  
  %this.updateBorders();
  
  // NOTE:  IN FLOOD FILL, IF YOU IMMEDIATELY TAKE OWNERSHIP OF
  // THE TILES THAT MATCHED THE NEWCOLOR, THEN IT WILL CONTINUE
  // TO SEARCH PAST THOSE.  PRETEND THAT YOUR GAME IS ONE-
  // DIMENSIONAL AND THE NUMBERS REPRESENT OWNERS AND THE
  // LETTERS REPRESENT COLORS
  //
  // +---+---+---+
  // |a 1|b 0|a 0|
  // +---+---+---+
  //
  // When you click color "b", you'll switch to
  // +---+---+---+
  // |b 1|b 0|a 0|
  // +---+---+---+
  //
  // The surrounding squares will get added to the stack.
  // INCORRECTLY, if you went ahead and switched the adjacent
  // tile's owner, you'd get
  // +---+---+---+
  // |b 1|b 1|a 0|
  // +---+---+---+
  // and you'd push the adjacent squares on.  Since "a" matches
  // the old color, you'd change it to "b", too!  WRONG!
  // (I say with embarassement, as I did that exact thing!)
  //
  // Therefore, I don't process ANY tile that already matches the
  // new color.
  //
  // The "floodOwn" function then uses a similar algorithm to go
  // over all the tiles, but it just changes the owner.
}

function fillGrid::floodFill( %this, %player, %newColor )
{   
  %maxX = %this.getTileCountX();
  %maxY = %this.getTileCountY();
  %start = %this.start[%player];
      
  %oldColor = getWord( %this.getTileCustomData( %start ), 0 );
  
  if(%newColor $= %oldColor)
    return;
  
  %stack = new ScriptObject() { class = Stack; };
  %stack.push( %start );

  // If there is anything in the stack, we'll process it.
  while( %stack.len > 0 )
  {
      // Get us an item off the stack.
      %top = %stack.pop();
      %x = getWord( %top, 0 );
      %y = getWord( %top, 1 );
      
      %currColor = getWord( %this.getTileCustomData( %top ), 0 );      
      %ownedBy = %this.owner( %top );
      
      // Only process tiles that meet the following criteria:
      // 1) Matched the old color
      // 2) Owned by me *OR* owned by nobody
      // NOTE: THIS LOGIC ALLOWS FOR A 4 PLAYER GAME IN THE FUTURE
      if( %currColor $= %oldColor &&
          ( %ownedBy == %player || %ownedBy == 0 ) )
      {
        // Fill the tile and set its custom data.
        %this.fillTile( %top, %newColor, %player );    
          
        // Push all adjacent squares onto the stack being
        // careful not to go past the edges of the grid.
        if( %x > 0 )
          %stack.push( (%x - 1) SPC %y );
        if( %x < %maxX - 1 )
          %stack.push( (%x + 1) SPC %y );
        if( %y > 0 )
          %stack.push( %x SPC (%y - 1) );
        if( %y < %maxY - 1 )
          %stack.push( %x SPC (%y + 1) );      
      }
  }
  
  %stack.delete();
}

function fillGrid::floodOwn( %this, %player )
{   
  %maxX = %this.getTileCountX();
  %maxY = %this.getTileCountY();
  %start = %this.start[%player];
      
  %oldColor = getWord( %this.getTileCustomData( %start ), 0 );
  
  %stack = new ScriptObject() { class = Stack; };
  %stack.push( %start );
  
  // TEMPORARY OBJECT TO KEEP TRACK OF WHAT SQUARES WE ALREADY
  // VISITED SO THAT WE DON'T GO BACK TO THEM!
  %visited = new ScriptObject();

  // If there is anything in the stack, we'll process it.
  while( %stack.len > 0 )
  {
      // Get us an item off the stack.
      %top = %stack.pop();
      %x = getWord( %top, 0 );
      %y = getWord( %top, 1 );
      
      if( %visited.seen[%top] ) continue;
      
      %visited.seen[%top] = true;
      
      %currColor = getWord( %this.getTileCustomData( %top ), 0 );      
      %ownedBy = %this.owner( %top );
      
      if( %currColor $= %oldColor && 
          ( %ownedBy == 0 || %ownedBy == %player ) )
      {
        // NOTE: THE "IF" PART OF THE FOLLOWING 2 LINES ISN'T NECESSARY,
        // BUT IF IT'S ALREADY OWNED BY THE PLAYER, THE DATA IS ALREADY
        // SET CORRECTLY, SO THIS IS JUST AN OPTIMIZATION.
        if( %ownedBy == 0 )
          %this.setTileCustomData( %top, %currColor SPC %player );
          
        // Push all adjacent squares onto the stack being
        // careful not to go past the edges of the grid.
        if( %x > 0 )
          %stack.push( (%x - 1) SPC %y );
        if( %x < %maxX - 1 )
          %stack.push( (%x + 1) SPC %y );
        if( %y > 0 )
          %stack.push( %x SPC (%y - 1) );
        if( %y < %maxY - 1 )
          %stack.push( %x SPC (%y + 1) );      
      }
  }
  
  %visited.delete();
  %stack.delete();
   
  %this.playerOwned[%player] = %this.numOwned(%player); 
  %this.setScores(); 
}

function fillGrid::numOwned( %this, %player )
{
  %maxX = %this.getTileCountX();
  %maxY = %this.getTileCountY();

  %count = 0;  
  
  for( %x = 0; %x < %maxX; %x++ )
  {
    for( %y = 0; %y < %maxY; %y++ )
    {
      if(getWord( %this.getTileCustomData( %x, %y ), 1 ) == %player )
        %count++;
    }
  }
  
  return %count;
}

//TODO funct to write to gui
function fillGrid::setScores(%this)
{
   player1OwnedLabel.text = %this.playerOwned[1];  
   player1ClicksLabel.text = %this.playerClicks[1];
   player2OwnedLabel.text = %this.playerOwned[2];  
   player2ClicksLabel.text = %this.playerClicks[2];
}

function fillGrid::fillTile(%this, %tilePos, %newColor, %player)
{   
   %this.setStaticTile( %tilePos, %newColor, 0 );
   %this.setTileCustomData( %tilePos, %newColor SPC %player);
}

function fillGrid::owner( %this, %posX, %posY )
{
  // NOTE 2: This allows us to pass just one parameter if that makes more sense.
  if( getWordCount( %posX ) == 2 )
    return getWord( %this.getTileCustomData( %posX ), 1 );
  
  return getWord( %this.getTileCustomData( %posX, %posY ), 1 );
}

function fillGrid::getRandomTileColor(%this)
{
   %index = getRandom(0,5);
   
   switch(%index)
   {
      case 0:
         return "aquaSquareImageMap";
      
      case 1:
         return "blueSquareImageMap";
         
      case 2:
         return "greenSquareImageMap";
         
      case 3:
         return "purpleSquareImageMap";
         
      case 4:
         return "redSquareImageMap";         
         
      case 5:
         return "yellowSquareImageMap";  
   }
}

// NOTE 2:
// For a given square, either a side is on the border or it is not.
// This nice "binary" definition allows us to assign each side to
// a bit in a binary number.  In case you don't have much background
// with bit manipulation:
//
// Base 10     Base 2
// -------     ------
//       1          1
//       2         10
//       4        100
//       8       1000
//
// thus, 10 (base 10) = 1010 (base 2)
//       7 (base 10) = 111 (base 2).
//
// We'll assign the top edge as 1, the right as 2, the bottom as 4,
// and the left as 8.  If the top and bottom edges are borders (1 + 4) = 5
// will represent the borders.
//
// At the same time, we have a tiled sprite where Sprite #5 has a border on
// the top and the bottom.  So it's a trivial mapping to change a border
// number into a sprite.

function fillGrid::updateBorders( %this )
{
  %maxX = %this.getTileCountX();
  %maxY = %this.getTileCountY();

  for( %x = 0; %x < %maxX; %x++ )
  {
    for( %y = 0; %y < %maxY; %y++ )
    {
      %owner = %this.owner( %x, %y );

      // If the tile isn't owned, we don't care about its borders.      
      if( %owner == 0 )
      {
        %this.borders.clearTile( %x, %y );
        continue;
      }
      
      %bits = 0;
      
      // NOTE 2: C++ does a lazy evaluation on "||" meaning that it
      // doesn't evaluate the right hand side if the left hand side
      // hand side is true.  TorqueScript evaluates both sides.
      // Therefore, I have to break the "or" condition out so that
      // I'm not trying to get custom data for (x, -1).
      
      // If at the top, or the top has a different onwer, set BIT 0
      if( %y == 0 )
        %bits += 1;
      else if( %owner != %this.owner( %x, %y - 1 ) )
        %bits += 1;
        
      // If at the right, or the right has a different owner, set BIT 1
      if( %x == %maxX - 1 )
        %bits += 2;
      else if( %owner != %this.owner( %x + 1, %y ) )
        %bits += 2;
        
      // If at the bottom, or the bottom has a different onwer, set BIT 2
      if( %y == %maxY - 1 )
        %bits += 4;
      else if( %owner != %this.owner( %x, %y + 1 ) )
        %bits += 4;
        
      // If at the left, or the left has a different owner, set BIT 3
      if( %x == 0 )
        %bits += 8;
      else if( %owner != %this.owner( %x - 1, %y ) )
        %bits += 8;
        
      %this.borders.setStaticTile( %x, %y, SquareBordersImageMap, %bits );
    }
  }
}