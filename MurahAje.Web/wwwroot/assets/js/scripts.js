$(function(){
	_ma_initDropdowns();
	_ma_initPage();
	$(".owl-carousel").owlCarousel({
		items:1,
		loop:true,
		autoplay:true,
		autoplayTimeout:5000,
		autoplayHoverPause:true
	});
});

function _ma_initPage(){
	$('.ui.sidebar').sidebar('attach events', '.toc.item');
	$('.accordion').accordion();
	$('.ui.checkbox').checkbox();
	$('.ma-popup').on('click',_ma_clearPopups);
	$('.ma-popup-block').on('click',function(e){
		e.stopPropagation();
	});
	_ma_onResizePage();
	$(window).resize(_ma_onResizePage);
	_ma_initMeterInputs();
	if (typeof Dropzone!='undefined') {
		Dropzone.autoDiscover = false;
		$('.ma-dropzone').addClass('dropzone').dropzone({ 
			addRemoveLinks : true 
		});
	}
}
function _ma_initDropdowns(){
	$('.ui.dropdown').dropdown({on: 'click'});
}

function _ma_onResizePage(){
	var ww = $(window).width();
	if (ww<=991) {
		$('.ma-content-block .ma-content-sidebar .ui.accordion').appendTo($('.ma-sidebar .ma-content-sidebar'));	
	} else {
		$('.ma-sidebar .ma-content-sidebar .ui.accordion').appendTo($('.ma-content-block .ma-content-sidebar'));
	}
	
}

function _ma_showSignRegPopup(){
	_ma_clearPopups();
	$('body').addClass('withLoginPopup');
}
function _ma_clearPopups(){
	$('body').removeClass('withLoginPopup');
}

function _ma_initMeterInputs(){
	var clearVals = function(obj) {
		var p = obj.parent();
		$('.item',p).removeClass('active');
		if (p.hasClass('selected')) {
			var v = parseInt(p.attr('data-value'),10);
			$('.item:lt('+v+')',p).addClass('active');
			setLabel(obj[0],(v-1));
		} else setLabel(obj[0],-1);
	}
	var setLabel = function(obj,val) {
		var p = $(obj).parent();
		if (p.hasClass('ma-ratingmeter')) {
			var idx = val==-2? $('.item',p).index($(obj)) : val;
			$('.ma-ratingscore>.value',p.parent()).text(((idx+1)/2).toFixed(1)).attr('class','value rating-'+(idx+1));
		}
	}
	$('.ma-meter-inputs .item').on('mouseover',function(){
		clearVals($(this));
		var p = $(this).parent();
		var o = $(this).addClass('active');
		o.prevAll().addClass('active');
		setLabel(this,-2);
	}).on('mouseout',function(){
		clearVals($(this));
	}).on('click',function(e){
		e.stopPropagation();
		var p = $(this).parent();
		var idx = $('.item',p).index($(this));
		p.attr('data-value',(idx+1)).addClass('selected');
		setLabel(this,-2);
		clearVals($(this));
		return false;
	})
}