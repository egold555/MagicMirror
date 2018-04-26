/*Async requests with no redirect*/
$(".should").click(function() {
	var returned = $.ajax({
        type: "POST",
        url: $(this).find("a").attr("href"),
    });
  return false;
});

/*Normal Redirect (Back Button)*/
$(".shouldnot").click(function() {
  window.location = $(this).find("a").attr("href"); 
  return false;
});

/*Prevent <a> Clicks*/
$('a').click(function(event) {
    event.preventDefault();
});