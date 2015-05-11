$(function() {
	$('a').focus(function() {
		$(this).blur();
	});
});

$(function(){
	var index = 0;
	var imgWidth = $('.pics_switch .pic_box a img').width();

	//alert(imgWidth);
	var len = $('.pics_switch .pic_box').length;
	//alert(len)
	var totalImgWidth = imgWidth*len;
	
	
	if($(window).width()<imgWidth) {
	    $('.ps_box .pics_switch').css({'width':$(".body_bj").width()});
	    $('.ps_box .pics_switch .pic_box').css({'width':$(".body_bj").width()});
	    $('.ps_box .pics_switch .pic_box a').css({'width':$(".body_bj").width()});
	    imgWidth = $(".body_bj").width();
	}
	
	
	$('.ps_box .pics_switch_clients ul li').css({'opacity':0.3});
	$('.ps_box .pics_switch_clients ul li span.current').css({'opacity':0.8});
	$('.pics_switch_clients li').hover(function() {
			$(this).addClass('hover');
		},function() {
			$(this).removeClass('hover');
		}
	);
	
	
	$('.ps_box .pics_switch .viewArrows').css({'opacity':0.4});
	$('.ps_box .pics_switch .viewArrows').hover(function() {
			$(this).fadeTo(200, 0.8);
		},function() {
			$(this).fadeTo(200, 0.4);
		}
	);
	
	
	$('.ps_box .pics_switch_clients ul li').css("opacity",0.9).mouseover(function() {
		index = $('.ps_box .pics_switch_clients ul li').index(this);
		showPics(index);
	}).eq(0).trigger("mouseover");
	
	
	$(".ps_box .prev").click(function() {
		index -= 1;
		if(index == -1) {index = len - 1;}
		showPics(index);
	});
	
	
	$(".ps_box .next").click(function() {
		index += 1;
		if(index == len) {index = 0;}
		showPics(index);
	});
	
	$('.ps_box .pb').css({'width':totalImgWidth});
	
	$('.ps_box .pb').hover(function() {
		clearInterval(picTimer);
	},function() {
		picTimer = setInterval(function() {
			showPics(index);
			index++;
			if(index == len) {index = 0;}
		},6000); 
	}).trigger("mouseleave");
	

	function showPics(index) {
		var nowLeft = -index*imgWidth; 
		$('.ps_box .pb').stop(true,false).animate({"marginLeft":nowLeft},1000,'easeInOutExpo'); 
		$('.ps_box .pics_switch_clients ul li span').stop(true,false).animate({"opacity":"0.4"},600).eq(index).stop(true,false).animate({"opacity":"1"},600); 
	}
	
});


