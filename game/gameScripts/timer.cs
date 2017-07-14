$startSeconds = 0;
$startMinutes = 0;
$startHours = 0;
$timerOn = true;

function startTimer()
{
   schedule(1000, 0, tickTimer);
}

function tickTimer()
{
   $startSeconds++;
   
   if($startSeconds >= 60)
   {
      $startSeconds = 0;
      $startMinutes++;
      
      if($startMinutes >= 60)
      {
         $startMinutes = 0;  
         $startHours++;
      }      
   }
   
   displayTime($startHours, $startMinutes, $startSeconds);   
   
   if($timerOn)
   {
      startTimer();  
   }
}

function stopTimer()
{
   $timerOn = false;
}

function resetTimer()
{
   $startSeconds = 0;
   $startMinutes = 0;
   $startHours = 0;
}

function displayTime(%hours, %minutes, %seconds)
{
   if(%hours < 10)
   {
      %formattedHours = "0" @ %hours;
   }
   else
   {
      %formattedHours = %hours;
   }
   
   if(%minutes < 10)
   {
      %formattedMinutes = "0" @ %minutes;
   }
   else
   {
      %formattedMinutes = %minutes;
   }
   
   if(%seconds < 10)
   {
      %formattedSeconds = "0" @ %seconds;
   }
   else
   {
      %formattedSeconds = %seconds;
   }
   
   timerLabel.text = %formattedHours @ ":" @ %formattedMinutes @ ":" @ %formattedSeconds;
}