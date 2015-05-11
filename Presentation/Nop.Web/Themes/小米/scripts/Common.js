/**
* common.js
* 2013.05.17
* kongge@office.weiphone.com
*/

//ds.base
; (function (global, document, undefined) {
    var
	rblock = /\{([^\}]*)\}/g,
	ds = global.ds = {
	    noop: function () { },
	    //Object
	    mix: function (target, source, cover) {
	        if (typeof source !== 'object') {
	            cover = source;
	            source = target;
	            target = this;
	        }
	        for (var k in source) {
	            if (cover || target[k] === undefined) {
	                target[k] = source[k];
	            }
	        }
	        return target;
	    },
	    //String
	    mixStr: function (sStr) {
	        var args = Array.prototype.slice.call(arguments, 1);
	        return String(sStr).replace(rblock, function (a, i) {
	            return args[i] != null ? args[i] : a;
	        });
	    },
	    trim: function (str) {
	        return String(str).replace(/^\s*/, '').replace(/\s*$/, '');
	    },
	    //Number
	    getRndNum: function (max, min) {
	        min = isFinite(min) ? min : 0;
	        return Math.random() * ((isFinite(max) ? max : 1000) - min) + min;
	    },
	    //BOM
	    scrollTo: (function () {
	        var
			duration = 480,
			view = $(global),
			setTop = function (top) { global.scrollTo(0, top); },
			fxEase = function (t) { return (t *= 2) < 1 ? .5 * t * t : .5 * (1 - (--t) * (t - 2)); };
	        return function (top, callback) {
	            top = Math.max(0, ~~top);
	            var
				tMark = new Date(),
				currTop = view.scrollTop(),
				height = top - currTop,
				fx = function () {
				    var tMap = new Date() - tMark;
				    if (tMap >= duration) {
				        setTop(top);
				        return (callback || ds.noop).call(ds, top);
				    }
				    setTop(currTop + height * fxEase(tMap / duration));
				    setTimeout(fx, 16);
				};
	            fx();
	        };
	    })(),
	    //DOM
	    loadScriptCache: {},
	    loadScript: function (url, callback, args) {
	        var cache = this.loadScriptCache[url];
	        if (!cache) {
	            cache = this.loadScriptCache[url] = {
	                callbacks: [],
	                url: url
	            };

	            var
				firstScript = document.getElementsByTagName('script')[0],
				script = document.createElement('script');
	            if (typeof args === 'object') {
	                for (var k in args) {
	                    script[k] = args[k];
	                }
	            }
	            script.src = url;
	            script.onload = script.onreadystatechange =
				script.onerror = function () {
				    if (/undefined|loaded|complete/.test(this.readyState)) {
				        script = script.onreadystatechange =
						script.onload = script.onerror = null;
				        cache.loaded = true;

				        for (var i = 0, len = cache.callbacks.length; i < len; i++) {
				            cache.callbacks[i].call(null, url);
				        }
				        cache.callbacks = [];
				    }
				};
	            firstScript.parentNode.insertBefore(script, firstScript);
	        }

	        if (!cache.loaded) {
	            if (typeof callback === 'function') {
	                cache.callbacks.push(callback);
	            }
	        }
	        else {
	            (callback || ds.noop).call(null, url);
	        }
	        return this;
	    },
	    requestAnimationFrame: (function () {
	        var handler = global.requestAnimationFrame || global.webkitRequestAnimationFrame ||
				global.mozRequestAnimationFrame || global.msRequestAnimationFrame ||
				global.oRequestAnimationFrame || function (callback) {
				    return global.setTimeout(callback, 1000 / 60);
				};
	        return function (callback) {
	            return handler(callback);
	        };
	    })(),
	    animate: (function () {
	        var
			easeOut = function (pos) { return (Math.pow((pos - 1), 3) + 1); };
	        getCurrCSS = global.getComputedStyle ? function (elem, name) {
	            return global.getComputedStyle(elem, null)[name];
	        } : function (elem, name) {
	            return elem.currentStyle[name];
	        };
	        return function (elem, name, to, duration, callback, easing) {
	            var
				from = parseFloat(getCurrCSS(elem, name)) || 0,
				style = elem.style,
				tMark = new Date(),
				size = 0;
	            function fx() {
	                var elapsed = +new Date() - tMark;
	                if (elapsed >= duration) {
	                    style[name] = to + 'px';
	                    return (callback || ds.noop).call(elem);
	                }
	                style[name] = (from + size * easing(elapsed / duration)) + 'px';
	                ds.requestAnimationFrame(fx);
	            };
	            to = parseFloat(to) || 0;
	            duration = ~~duration || 200;
	            easing = easing || easeOut;
	            size = to - from;
	            fx();
	            return this;
	        };
	    })(),
	    //Cookies
	    getCookie: function (name) {
	        var ret = new RegExp('(?:^|[^;])' + name + '=([^;]+)').exec(document.cookie);
	        return ret ? decodeURIComponent(ret[1]) : '';
	    },
	    setCookie: function (name, value, expir) {
	        var cookie = name + '=' + encodeURIComponent(value);
	        if (expir !== void 0) {
	            var now = new Date();
	            now.setDate(now.getDate() + ~~expir);
	            cookie += '; expires=' + now.toGMTString();
	        }
	        document.cookie = cookie;
	    },
	    //Hacker
	    transitionSupport: (function () {
	        var
			name = '',
			prefixs = ['', 'webkit', 'moz', 'ms', 'o'],
			docStyle = (document.documentElement || document.body).style;
	        for (var i = 0, len = prefixs.length; i < len; i++) {
	            name = prefixs[i] + (prefixs[i] !== '' ? 'Transition' : 'transition');
	            if (name in docStyle) {
	                return {
	                    propName: name,
	                    prefix: prefixs[i]
	                };
	            }
	        }
	        return null;
	    })(),
	    isIE6: !-[1, ] && !global.XMLHttpRequest
	};
    //lazyResize
    ds.mix((function () {
        var timer, bound, callbacks = [];
        function resizeHandler(evt) {
            if (callbacks.length > 0) {
                $.each(callbacks, function (i, callback) {
                    callback.call(global, evt);
                });
            }
        }
        return {
            lazyResize: function (callback) {
                if (typeof callback === 'function') {
                    callbacks.push(callback);

                    if (!bound) {
                        $(global).bind('resize.lazyResize', function () {
                            clearTimeout(timer);
                            timer = setTimeout(resizeHandler, 160);
                        });
                        bound = true;
                    }
                }
                return this;
            }
        };
    })());
})(this, document);

/**
* jquery.tabs.base.js
* create: 2011.12.05
* update: 2013.05.25
* admin@laoshu133.com
*/
; (function (global, document, $) { var noop = function () { }, fakePanel = { show: noop, hide: noop }, _ops = { index: 0, delay: 0, shellTag: 'a', preventDefault: true, event: 'mouseenter', focusClass: 'current', onbeforeselect: noop, onselect: noop }; $.fn.tabs = function (ops) { ops = ops || {}; for (var k in _ops) { if (typeof ops[k] === 'undefined') { ops[k] = _ops[k]; } } var self = this, prevTrigger, timer; function getPanel(trigger) { var id = trigger.attr('href'); id = id.slice(id.lastIndexOf('#') + 1); return id ? $('#' + id) : fakePanel; } function selectHandler(trigger) { if (trigger !== prevTrigger) { var panel = trigger.data('@panel'); if (!panel) { panel = getPanel(trigger); trigger.data('@panel', panel); } if (ops.onbeforeselect.call(self, panel, trigger) !== false) { if (prevTrigger) { prevTrigger.removeClass(ops.focusClass); prevTrigger.data('@panel').hide(); } panel.show(); trigger.addClass(ops.focusClass); ops.onselect.call(self, panel, trigger); prevTrigger = trigger; } } }; this.delegate(ops.shellTag, ops.event, function (e) { if (ops.preventDefault && e.type === 'click') { e.preventDefault(); } var trigger = $(this); if (ops.event !== 'mouseenter' || ops.delay <= 0) { selectHandler(trigger); } else { clearTimeout(timer); timer = setTimeout(function () { selectHandler(trigger); }, ops.delay); trigger.one('mouseleave', function () { clearTimeout(timer); }); } }); if (ops.preventDefault && ops.event !== 'click') { this.delegate(ops.shellTag, 'click', function (e) { e.preventDefault(); }); } var defaultTrigger; this.find(ops.shellTag).each(function (i) { var trigger = $(this), panel = getPanel(trigger); trigger.data('@panel', panel); if (i === 0 || i === ops.index) { defaultTrigger = trigger; } else { trigger.removeClass(ops.focusClass); panel.hide(); } }); selectHandler(defaultTrigger); return this; }; })(this, this.document, jQuery);

/**
* jquery.Slider.base.js
* create: 2011.12.13
* update: 2013.05.31
* admin@laoshu133.com
*/
; (function (global, document, $, undefined) { var noop = function () { }, slider = global.Slider = function (ops) { this.init(ops || {}); }; slider.prototype = { constructor: slider, _ops: { length: 3, shell: null, auto: true, cName: 'left', duration: 200, unitSize: 500, delay: 5000, easing: 'swing', onbeforeplay: noop, onplay: noop }, init: function (ops) { var self = this, _ops = this._ops; for (var k in _ops) { if (typeof ops[k] === 'undefined') { ops[k] = _ops[k]; } } this.shell = $(ops.shell); this.ops = ops; this.index = -1; this.play(0); if (ops.auto) { this.shell.bind('mouseenter.slider', function () { self.stopAuto(); }).bind('mouseleave.slider', function () { self.autoPlay(); }); } }, play: function (inx) { var self = this, ops = this.ops; inx = ~~(inx === undefined ? this.index + 1 : inx) % ops.length; inx = inx < 0 ? ops.length + inx : inx; if (inx === this.index || ops.onbeforeplay.call(this, inx, this.index) === false) { return this; } var aniOps = {}; aniOps[ops.cName] = -inx * ops.unitSize; this.shell.stop().animate(aniOps, ops.duration, ops.easing, function () { ops.onplay.call(self, inx, self.index); self.index = inx; if (ops.auto) { self.autoPlay(); } }); clearTimeout(this.timer); return this; }, next: function () { return this.play(); }, prev: function () { return this.play(this.index - 1); }, autoPlay: function () { var self = this; this.ops.auto = true; clearTimeout(this.timer); this.timer = setTimeout(function () { self.play(); }, this.ops.delay); }, stopAuto: function () { clearTimeout(this.timer); this.ops.auto = false; } }; })(window, document, jQuery);

/** 
 * jQuery.lazyLoad.js
 * kongge@office.weiphone.com
 * 2012.05.23
**/
; (function (global, document, $, undefined) { var noop = function () { }, _defOps = { container: global, scrollInterval: 90, effect: 'show', effectDuration: 0, offset: 0, onstartload: noop, onload: noop, ondone: noop }, _startLoading = function (elem, ops, count, length) { elem.one('load', function () { elem.hide()[ops.effect](ops.effectDuration); ops.onload.call(elem); count >= length && ops.ondone.call(null, ops); elem = null; }).attr('src', elem.attr('longdesc')); }; $.fn.lazyLoad = function (setting) { var ops = setting || {}; $.each(_defOps, function (k, val) { if (ops[k] === undefined) { ops[k] = val; } }); var count = 0, _cache = [], timer = null, view = $(ops.container), isWindow = ops.container === global; function _checkFn() { var len = _cache.length, viewHeight = view.height(), viewTop = isWindow ? view.scrollTop() : view.offset().top, elem, i, top, height, isAbove, isBelow; for (i = 0; i < len && (elem = _cache[i]) ; i++) { height = elem.height(); top = elem.offset().top - viewTop; isBelow = top >= 0 && top <= viewHeight + ops.offset; isAbove = top + height <= viewHeight + ops.offset && top + height >= 0; if (!elem.loaded && (isAbove || isBelow)) { count++; elem.loaded = true; ops.onstartload.call(elem) !== false && _startLoading(elem, ops, count, len); } } if (count >= len) { view.unbind('scroll', checkFn); (isWindow ? view : $(window)).unbind('resize', checkFn); } } function checkFn() { clearTimeout(timer); timer = setTimeout(_checkFn, ops.scrollInterval); } this.each(function (i) { var elem = $(this); if (!elem.data('lazyLoaded') && elem.attr('longdesc')) { _cache.push(elem); } }).data('lazyLoad', { ops: ops, _cache: _cache }); if (_cache.length > 0) { checkFn(); view.scroll(checkFn); (isWindow ? view : $(window)).bind('resize', checkFn); } return this; }; })(this, document, jQuery);

/**
* ds.tmpl.js
* create: 2013.01.10
* update: 2013.09.26
* admin@laoshu133.com
**/
; (function (global) { var ds = global.ds || (global.ds = {}); var rarg1 = /\$1/g, rgquote = /\\"/g, rbr = /([\r\n])/g, rchars = /(["\\])/g, rdbgstrich = /\\\\/g, rfuns = /<%\s*(\w+|.)([\s\S]*?)\s*%>/g, rbrhash = { '10': 'n', '13': 'r' }, helpers = { '=': { render: '__.push($1);' } }; ds.tmpl = function (tmpl, data) { var render = new Function('_data', 'var __=[];__.data=_data;' + 'with(_data){__.push("' + tmpl.replace(rchars, '\\$1').replace(rfuns, function (a, key, body) { body = body.replace(rbr, ';').replace(rgquote, '"').replace(rdbgstrich, '\\'); var helper = helpers[key], tmp = !helper ? key + body : typeof helper.render === 'function' ? helper.render.call(ds, body, data) : helper.render.replace(rarg1, body); return '");' + tmp + '__.push("'; }).replace(rbr, function (a, b) { return '\\' + (rbrhash[b.charCodeAt(0)] || b); }) + '");}return __.join("");'); return data ? render.call(data, data) : render; }; ds.tmpl.helper = helpers; })(this);

/**
* jquery.mask.js
* create: 2012.12.04
* update: 2012.12.13
* admin@laoshu133.com
*/
; (function (global, document, $, undefined) { var DOC = $(document), _noop = function () { }, ds = global.ds || (global.ds = {}); var _uuid = 0, _ops = { id: null, anim: true, animDuration: 320, zIndex: 999, opacity: 0.8, background: '#000', onshow: _noop, onhide: _noop, onclick: _noop }, Mask = ds.Mask = function (ops) { return this.init(ops); }; Mask._cache = {}; Mask.prototype = { constructor: Mask, init: function (ops) { this.ops = ops = ops || {}; $.each(_ops, function (k, val) { typeof ops[k] === 'undefined' && (ops[k] = val); }); var id = ops.id; if (typeof id !== 'string') { id = 'ds_mask_' + (++_uuid); } this.id = id; if (Mask._cache[id]) { var _mask = Mask._cache[id]; _mask.ops = ops; return _mask; } Mask._cache[id] = this; return this; }, _getElem: function () { var elem = this.elem, self = this; if (!elem) { var ops = this.ops, css = 'border:0;margin:0;padding:0;height:100%;width:100%;left:0;top:0;opacity:0;'; elem = this.elem = $(document.createElement('div')); elem.bind('click.jquery_mask', function () { self.ops.onclick.call(self); }); elem[0].style.cssText = css + 'display:none;position:fixed;background:' + ops.background + ';z-index:' + ops.zIndex; if (!Mask.fixedPositionSupport()) { elem[0].style.position = 'absolute'; css += 'position:absolute;filter:Alpha(opacity=0);'; elem[0].innerHTML = '<iframe src="javascript:false" frameborder="0" height="100%" width="100%" style="' + css + 'z-index:1;"></iframe><div style="' + css + 'background:#fff;z-index:9;"></div>'; } elem[0].id = this.id; elem.appendTo('body'); } return elem; }, show: function (opacity) { var self = this, ops = this.ops, elem = this._getElem(); opacity = typeof opacity === 'number' ? opacity : ops.opacity; if (!Mask.fixedPositionSupport()) { elem.css('height', DOC.height()); } this.elem.css('background', ops.background).show(); if (ops.anim) { elem.stop().animate({ opacity: opacity }, ops.animDuration, function () { ops.onshow.call(self, opacity); }); } else { elem.stop().css('opacity', opacity); ops.onshow.call(this, opacity); } return this; }, hide: function () { var self = this, ops = this.ops, elem = this._getElem(); if (ops.onhide.call(this) !== false) { if (ops.anim) { elem.stop().animate({ opacity: 0 }, ops.animDuration, function () { this.style.display = 'none'; }); } else { elem[0].style.display = 'none'; } } return this; }, destory: function () { if (this.elem) { this.elem.remove(); this.elem = null; } delete Mask._cache[this.id]; } }; Mask.fixedPositionSupport = function () { if (typeof $.support.fixedPosition === 'boolean') { return $.support.fixedPosition; } var div, ret = false, body = document.body, css = 'border:0;margin:0;padding:0;position:fixed;left:0;top:20px;'; if (body) { div = document.createElement('div'); div.style.cssText = css + 'position:absolute;top:0;'; div.innerHTML = '<div style="' + css + '"></div>'; body.insertBefore(div, body.firstChild); ret = $.support.fixedPosition = div.firstChild.offsetTop === 20; body.removeChild(div); } return ret; }; ds.mask = $.mask = function (opacity, background) { var ops = typeof opacity === 'object' ? opacity : { opacity: opacity, background: background }; ops.id = 'ds_global_mask'; return new Mask(ops).show(); }; })(this, this.document, jQuery);

/**
* ds.dialog.js
* create: 2012.12.05
* update: 2012.12.14
* admin@laoshu133.com
*/
; (function (global, document, $, undefined) { var view = $(global), DOC = $(document), ds = global.ds || (global.ds = {}); var _guid = 0, _noop = function () { }, _tmpl = '<div class="ds_dialog_outer"><table class="ds_dialog_border"><tr><td class="ds_dialog_tl"></td><td class="ds_dialog_tc"></td><td class="ds_dialog_tr"></td></tr><tr><td class="ds_dialog_ml"></td><td class="ds_dialog_mc"><div class="ds_dialog_inner"><table class="ds_dialog_panel"><tr><td colspan="2" class="ds_dialog_header"><div class="ds_dialog_title"><h3></h3></div><div class="ds_dialog_close"><a href="javascript:;">\u00d7</a></div></td></tr><tr><td class="ds_dialog_icon" style="display:none"><div class="ds_dialog_icon_bg"></div></td><td class="ds_dialog_main"><div id="{id}_content" class="ds_dialog_content"></div></td></tr><tr><td colspan="2" class="ds_dialog_footer"><div class="ds_dialog_buttons"></div></td></tr></table></div></td><td class="ds_dialog_mr"></td></tr><tr><td class="ds_dialog_bl"></td><td class="ds_dialog_bc"></td><td class="ds_dialog_br"></td></tr></table></div>', _ops = { id: null, title: null, content: '', className: null, padding: '20px 25px', height: 'auto', width: 'auto', left: '50%', top: '40%', zIndex: 1990, icon: null, iconBasePath: global.iconBasePath || 'images/', buttons: null, follow: null, followOffset: null, autoOpen: true, esc: true, lock: true, lockOpacity: 0.6, lockColor: '#000', showCloseButton: true, drag: true, fixed: true, anim: true, animDuration: 320, timeout: 0, oninit: _noop, onopen: _noop, onfocus: _noop, onmaskclick: _noop, onhide: _noop, onclose: _noop, yesText: '\u786e\u5b9a', noText: '\u53d6\u6d88', onyes: null, onno: null }, Dialog = ds.Dialog = function (ops) { return this.init(ops || {}); }; $.extend(Dialog, { items: {}, defaults: _ops, currIndex: 1990, defaultTmpl: _tmpl, activeDialog: null, dialogQueue: [], addDialog: function (dialog) { var queue = this.dialogQueue; this.removeDialog(dialog); queue.push(dialog); dialog.inQueue = true; dialog.queueIndex = queue.length - 1; }, removeDialog: function (dialog) { var i = dialog.queueIndex, queue = this.dialogQueue; if (dialog.inQueue) { queue.splice(i, 1); for (var len = queue.length; i < len; i++) { queue[i].queueIndex--; } } dialog.inQueue = false; dialog.queueIndex = -1; }, maskQueue: [], addMask: function (dialog) { this.removeMask(dialog); this.maskQueue.push(dialog); dialog.maskIndex = this.maskQueue.length - 1; }, removeMask: function (dialog) { var i = dialog.maskIndex, maskQueue = this.maskQueue; if (dialog.locked) { maskQueue.splice(i, 1); for (var len = maskQueue.length; i < len; i++) { maskQueue[i].maskIndex--; } } dialog.maskIndex = -1; } }); Dialog.items = {}; Dialog.defaults = _ops; Dialog.currZIndex = 1990; Dialog.defaultTmpl = _tmpl; Dialog.prototype = { version: '2.0', constructor: Dialog, init: function (ops) { var id = typeof ops.id === 'string' ? ops.id : ('ds_dialog_' + (++_guid)); if (Dialog.items[id]) { var dialog = Dialog.items[id], rOps = dialog.ops; var opsChangeMaps = { left: 1, top: 1, follow: 1, onopen: 1, onfocus: 1, onhide: 1, onclose: 1, onyes: 1, onno: 1, esc: 2, lock: 2, anim: 2, drag: 2, fixed: 2, autoOpen: 2, icon: 3, content: 3 }; $.each(ops, function (k, val) { if (k in opsChangeMaps && val !== void 0) { var type = opsChangeMaps[k]; if (type === 2) { val = !!val; } if (val !== rOps[k]) { rOps[k] = val; type === 3 && dialog[k](val); } } }); dialog[rOps.drag ? 'initDrag' : 'releaseDrag'](); dialog[rOps.fixed ? 'initFixed' : 'releaseFixed'](); rOps.autoOpen && dialog.show(); return dialog; } $.each(_ops, function (k, val) { typeof ops[k] === 'undefined' && (ops[k] = val); }); if (!$.isArray(ops.followOffset)) { ops.followOffset = [0, 0]; } this.id = id; this.ops = ops; this._getElem(ops); Dialog.items[id] = this; this._initEvent(); this.padding(ops.padding).size(ops.width, ops.height).title(ops.title).button(ops.buttons).icon(ops.icon).content(ops.content).timeout(ops.timeout); typeof ops.oninit === 'function' && ops.oninit.call(this); ops.fixed && this.initFixed && this.initFixed(); ops.drag && this.initDrag && this.initDrag(); ops.autoOpen && this.show(); }, _getElem: function (ops) { var shell = document.createElement('div'); shell.id = this.id; shell.tabIndex = -1; shell.style.position = 'absolute'; shell.className = 'ds_dialog' + (ops.className ? ' ' + ops.className : ''); shell.innerHTML = Dialog.defaultTmpl.replace(/\{id\}/g, this.id); shell = this.shell = $(shell); this.contentShell = shell.find('.ds_dialog_content').eq(0); this.buttonShell = shell.find('.ds_dialog_buttons').eq(0); this.titleShell = shell.find('.ds_dialog_title').eq(0); this.closeShell = shell.find('.ds_dialog_close').eq(0); this.iconShell = shell.find('.ds_dialog_icon').eq(0); this.closeShell[ops.showCloseButton ? 'show' : 'hide'](); var buttons = ops.buttons; if (!$.isArray(buttons)) { buttons = ops.buttons = []; } if (ops.onyes === true || typeof ops.onyes === 'function') { buttons.push({ autoFocus: true, text: ops.yesText, className: 'ds_dialog_yes', onclick: function () { return $.type(ops.onyes) === 'function' && ops.onyes.call(this) === false || !ops.onyes ? false : this.hide(); } }); } if (ops.onno === true || typeof ops.onno === 'function') { buttons.push({ text: ops.noText, className: 'ds_dialog_no', onclick: function () { return $.type(ops.onno) === 'function' && ops.onno.call(this) === false || !ops.onno ? false : this.hide(); } }); } return shell; }, _initEvent: function () { var self = this; this.closeShell.bind('click', function (e) { e.stopPropagation(); e.preventDefault(); self.hide(); }); this.shell.bind('mousedown', function () { self.focus(); }); }, isOpen: false, show: function (left, top) { var ops = this.ops, shell = this.shell; if (!this._inDOM) { this._inDOM = true; shell.appendTo('body'); } if (!this.isOpen) { ops.lock && this.lock(); !ops.follow || arguments.length >= 1 ? this.position(left || ops.left, top || ops.top) : this.follow(ops.follow, ops.followOffset[0], ops.followOffset[1]); ops.anim ? shell.stop().animate({ opacity: 1 }, ops.animDuration) : shell.stop().css('opacity', 1); shell.css('display', 'block'); this.isOpen = true; ops.onopen.call(this); } return this.focus(); }, hide: function () { var ops = this.ops; if (this.isOpen) { this.isOpen = false; if (ops.onhide.call(this) !== false) { this.blur().unlock(); ops.anim ? this.shell.stop().animate({ opacity: 0 }, ops.animDuration, function () { this.style.display = 'none'; }) : this.shell.stop().hide(); Dialog.removeDialog(this); } else { this.isOpen = true; } } return this; }, focus: function () { var ops = this.ops; if (this.isOpen && !this.hasFocus && ops.onfocus.call(this) !== false) { Dialog.activeDialog && Dialog.activeDialog._blur(); Dialog.addDialog(this); var shell = this.shell; this.zIndex = Dialog.currZIndex = Math.max(++Dialog.currZIndex, this.ops.zIndex); shell.addClass('ds_dialog_active').css('zIndex', this.zIndex); if ('focus' in shell[0]) { shell[0].focus(); this.focusButton && this.focusButton.focus(); } Dialog.activeDialog = this; this.hasFocus = true; } return this; }, _blur: function () { this.hasFocus = false; this.shell.removeClass('ds_dialog_active'); }, blur: function () { if (this.hasFocus) { this._blur(); var queue = Dialog.dialogQueue, len = queue.length; if (len > 1) { queue[len - 2].focus(); } else { Dialog.activeDialog = null; } } return this; }, close: function () { var self = this, ops = this.ops; if (this.shell && ops.onclose.call(this) !== false) { if (ops.anim) { this.hide(); setTimeout(function () { self.destory(); }, ops.animDuration); } else { this.hide().destory(); } } }, timeout: function (delay) { var self = this; clearTimeout(this.timer); if (~~delay > 0) { this.timer = setTimeout(function () { self.close(); }, ~~delay * 1000); } return this; }, destory: function () { delete Dialog.items[this.id]; if (this.shell) { this.shell.hide().remove(); } this.shell = this.contentShell = this.buttonShell = this.titleShell = this.closeShell = this.iconShell = this.focusButton = null; }, _getPositionLimit: function () { var ops = this.ops, shell = this.shell, offset = shell.offset(), viewScrollLeft = view.scrollLeft(), viewScrollTop = view.scrollTop(), shellWidth = shell.outerWidth(), shellHeight = shell.outerHeight(), viewWidth = view.width(), viewHeight = view.height(), minTop = ops.fixed ? 0 : viewScrollTop, minLeft = ops.fixed ? 0 : viewScrollLeft, maxTop = minTop + viewHeight - shellHeight, maxLeft = minLeft + viewWidth - shellWidth; return { minTop: minTop, minLeft: minLeft, maxTop: maxTop, maxLeft: maxLeft, viewHeight: viewHeight, viewWidth: viewWidth, height: shellHeight, width: shellWidth, top: offset.top, left: offset.left, viewScrollTop: viewScrollTop, viewScrollLeft: viewScrollLeft }; }, _position: function (left, top) { var ops = this.ops, style = this.shell[0].style; style.left = left + 'px'; style.top = top + 'px'; }, position: function (left, top) { if (arguments.length < 1) { return this.shell.offset(); } var ops = this.ops, rper = /(\d+\.?\d+)%/, limit = this._getPositionLimit(); if (rper.test(left)) { left = (limit.viewWidth - limit.width) * parseFloat(RegExp.$1) / 100; left += ops.fixed ? 0 : limit.viewScrollLeft; } if (rper.test(top)) { top = (limit.viewHeight - limit.height) * parseFloat(RegExp.$1) / 100; top += ops.fixed ? 0 : limit.viewScrollTop; } left = Math.max(limit.minLeft, Math.min(limit.maxLeft, left)); top = Math.max(limit.minTop, Math.min(limit.maxTop, top)); this._position(left, top); return this; }, size: function (width, height) { this.contentShell.css('width', width).css('height', height); return this; }, padding: function (padding) { this.contentShell.css('padding', padding); return this; }, follow: (function () { var _offsetMaps = { left: function () { return 0; }, right: function (shell, target) { return target.outerWidth() - shell.outerWidth(); }, top: function (shell, target) { return -shell.outerHeight(); }, bottom: function (shell, target) { return target.outerHeight(); } }, _getOffset = function (shell, target, _offset) { if (_offsetMaps[_offset[0]]) { _offset[0] = _offsetMaps[_offset[0]](shell, target); } if (_offsetMaps[_offset[1]]) { _offset[1] = _offsetMaps[_offset[1]](shell, target); } return [~~_offset[0], ~~_offset[1]]; }; return function (target, left, top) { target = $(target); var pos = $(target).offset(), offset = _getOffset(this.shell, target, [left, top]); if (this.ops.fixed) { pos.left -= view.scrollLeft(); pos.top -= view.scrollTop(); } return this.position(pos.left + offset[0], pos.top + offset[1]); }; })(), title: function (title) { if (!title) { this.titleShell.hide(); } else { this.titleShell.html('<h3>' + title + '</h3>'); this.titleShell.show(); } return this; }, content: function (content) { this.contentShell.html(content); return this.position(this.ops.left, this.ops.top); }, button: function (buttons) { var self = this, ops = this.ops, shell = this.buttonShell.hide(); if ($.isArray(buttons) && buttons.length > 0) { $.each(buttons, function (i) { var btn = document.createElement('button'); btn.disabled = this.disabled; btn.innerHTML = '<span>' + this.text + '</span>'; btn.className = this.className + (this.disabled ? ' disabled' : ''); btn = $(btn); var onclick = this.onclick; btn.bind('click', function (e) { e.stopPropagation(); typeof onclick === 'function' && onclick.call(self, this, e); }); if (this.autoFocus) { self.focusButton = btn; } shell.append(btn); }); shell.show(); } return this; }, icon: function (iconUrl) { var shell = this.iconShell; if (!!iconUrl) { shell.find('div')[0].style.backgroundImage = 'url(' + this.ops.iconBasePath + iconUrl + ')'; shell.show(); } else { shell.hide(); } return this; }, lock: function (opacity, lockColor) { var ops = this.ops, mask = Dialog.mask; if (!mask) { mask = Dialog.mask = new ds.Mask({ opacity: _ops.lockOpacity, background: _ops.lockColor, onclick: function () { var dialog = Dialog.activeDialog; dialog && dialog.ops.onmaskclick.call(dialog); } }); } Dialog.addMask(this); lockColor = lockColor || ops.lockColor; if (mask._lastBackground !== lockColor) { mask._lastBackground = lockColor; mask._getElem().css('background', lockColor); } mask.show(opacity || ops.opacity); this.locked = true; return this; }, unlock: function () { if (this.locked) { Dialog.removeMask(this); var maskQueue = Dialog.maskQueue; if (maskQueue.length > 0) { maskQueue[maskQueue.length - 1].lock(); } else { Dialog.mask.hide(); } this.locked = false; } return this; } }; (function () { var rinput = /INPUT|TEXTAREA/i; DOC.bind('keydown', function (e) { var dialog = Dialog.activeDialog; if (dialog && dialog.ops.esc && e.keyCode === 27 && !rinput.test(e.target.nodeName)) { dialog.hide(); } }); })(); $.extend(Dialog.prototype, (function () { var supportFixed; return { initFixed: function () { if (typeof supportFixed !== 'boolean') { supportFixed = ds.Mask.fixedPositionSupport(); } if (!this._initFixed) { var shell = this.shell; shell[0].style.position = supportFixed ? 'fixed' : 'absolute'; this.ops.fixed = true; if (!supportFixed) { this.ops.fixed = false; } this._initFixed = true; } return this; }, releaseFixed: function () { if (this._initFixed) { var pos = this.position(), scrollLeft = view.scrollLeft(), scrollTop = view.scrollTop(); this.position(pos.left + scrollLeft, pos.top + scrollTop); this.shell[0].style.position = 'absolute'; this.ops.fixed = false; } return this; } }; })()); $.extend(Dialog.prototype, (function () { var html = document.documentElement, hasCapture = 'setCapture' in html, hasCaptureEvt = 'onlosecapture' in html, clearRanges = 'getSelection' in global ? function () { global.getSelection().removeAllRanges(); } : function () { document.selection && document.selection.empty(); }, isNotDragArea = function (dialog, target) { var content = dialog.contentShell[0], close = dialog.closeShell[0]; if (target == content || $.contains(content, target) || target === close || $.contains(close, target) || $.contains(dialog.buttonShell[0], target)) { return true; } return false; }; return { initDrag: function () { if (!this._dragHandler) { var self = this; this._dragHandler = function (e) { if (e.button !== 0 && e.button !== 1 || isNotDragArea(self, e.target)) { return; } var ops = self.ops, shell = self.shell, limit = self._getPositionLimit(), currDrag = Dialog._currDrag = { limit: limit, dialog: self, offsetTop: e.pageY - limit.top, offsetLeft: e.pageX - limit.left, onmousemove: function (e) { var top = e.pageY - currDrag.offsetTop, left = e.pageX - currDrag.offsetLeft; top -= ops.fixed ? limit.viewScrollTop : 0; left -= ops.fixed ? limit.viewScrollLeft : 0; top = Math.min(limit.maxTop, top); left = Math.min(limit.maxLeft, left); ops.top = Math.max(limit.minTop, top); ops.left = Math.max(limit.minLeft, left); self._position(ops.left, ops.top); clearRanges(); }, onmouseup: function () { hasCapture && shell[0].releaseCapture(); hasCaptureEvt ? shell.unbind('losecapture', currDrag.onmouseup) : view.unbind('blur', currDrag.onmouseup); DOC.unbind('mousemove', currDrag.onmousemove).unbind('mouseup', currDrag.onmouseup); shell.removeClass('ds_dialog_drag'); Dialog._currDrag = null; } }; hasCaptureEvt ? shell.bind('losecapture', currDrag.onmouseup) : view.bind('blur', currDrag.onmouseup); hasCapture && shell[0].setCapture(); DOC.bind('mousemove', currDrag.onmousemove).bind('mouseup', currDrag.onmouseup); shell.addClass('ds_dialog_drag'); return false; }; this.shell.bind('mousedown', this._dragHandler); } return this; }, releaseDrag: function () { if (this._dragHandler) { this.shell.unbind('mousedown', this._dragHandler); this._dragHandler = null; } return this; } }; })()); ds.dialog = function (ops, onyes, onno) { if (typeof ops === 'string') { ops = { content: ops, title: arguments[1] || '\u6d88\u606f\u63d0\u793a', onyes: onyes, onno: onno }; } return new Dialog(ops || {}); }; $.extend(ds.dialog, { items: Dialog.items, alert: function (content, onhide, icon) { if (typeof onhide === 'string') { icon = onhide; } return new Dialog({ id: 'ds_dialog_alert', fixed: true, left: '50%', top: '40%', icon: icon ? icon : 'info.png', title: '\u6d88\u606f\u63d0\u793a', content: content, buttons: [{ text: '\u786e\u5b9a', autoFocus: true, className: 'ds_dialog_yes', onclick: function () { this.close(); } }], onhide: typeof onhide === 'function' ? onhide : function () { } }); }, confirm: function (content, onyes, onno, onhide) { return new Dialog({ id: 'ds_dialog_confirm', fixed: true, left: '50%', top: '40%', icon: 'confirm.png', title: '\u6d88\u606f\u786e\u8ba4', content: content, onyes: onyes || true, onhide: onhide, onno: onno || true }); }, prompt: function (content, onyes, defaultValue) { return new Dialog({ id: 'ds_dialog_prompt', fixed: true, left: '50%', top: '40%', icon: 'confirm.png', title: '\u6d88\u606f\u786e\u8ba4', content: '<p style="margin:0;padding:0 0 5px;">' + content + '</p><div><input type="text" style="color:#333;font-size:12px;padding:.42em .33em;width:18em;" /></div>', onopen: function () { var self = this, input = this._input; if (!input) { input = this._input = this.contentShell.find('input'); input.bind('keydown', function (e) { if (e.keyCode === 13 && self.focusButton) { self.focusButton.trigger('click'); return false; } }); } input.val(defaultValue || ''); }, onfocus: function () { var input = this._input[0]; setTimeout(function () { input.select(); input.focus(); }, 32); }, onclose: function () { this._input = null; }, onyes: function () { var input = this._input[0]; return typeof onyes === 'function' ? onyes.call(this, input.value, input) : void 0; }, onno: true }); }, tips: function (content, timeout, follow, lock) { if (typeof follow === 'boolean') { lock = follow; follow = null; } return new Dialog({ id: 'ds_dialog_tips', fixed: true, esc: false, lock: !!lock, follow: follow, followOffset: [0, 'bottom'], drag: false, content: content, padding: '12px 50px', showCloseButton: false, timeout: timeout || 0 }); } }); })(this, this.document, jQuery);

/**
* jQuery.tips.js
* create: 2012.12.05
* update: 2013.07.22
* admin@laoshu133.com
*/
; (function (global, $) { var ds = global.ds || (global.ds = {}); var uuid = 0, tipsTmpl = '<div class="ds_tips_exts"><i class="ds_tips_arrow"><b></b></i></div><div class="ds_tips_content">{html}</div>', _ops = { id: null, content: '', follow: null, width: 'auto', height: 'auto', className: '', animate: true, animateDuration: 400, direction: 'bottom', offset: [0, 0, 0, 0], autoDisplay: true }; ds.Tips = $.Tips = function (ops) { this.init(ops || {}); }; ds.Tips.prototype = { constructor: ds.Tips, init: function (ops) { $.each(_ops, function (k, val) { if (ops[k] === void 0) { ops[k] = _ops[k] && _ops[k].slice ? _ops[k].slice(0) : _ops[k]; } }); var elem = document.createElement('div'); elem.id = ops.id ? ops.id : 'ds_tips_' + (++uuid); elem.style.cssText = 'display:none;position:absolute;opacity:0;filter:Alpha(opacity=0)'; elem.className = 'ds_tips' + (ops.className ? ' ' + ops.className : ''); elem.innerHTML = tipsTmpl.replace('{html}', ops.content); this.ops = ops; this.follow = $(ops.follow); this.shell = $(elem).appendTo('body'); this.arrow = this.shell.find('.ds_tips_arrow').eq(0); this.contentShell = this.shell.find('.ds_tips_content').eq(0); ops.autoDisplay && this.show(); }, content: function (html) { this.contentShell.html(html); return this; }, show: function (direction, offset) { var ops = this.ops, shell = this.shell; if (!offset || !offset.length) { offset = ops.offset; } shell.stop().css('width', ops.width).css('height', ops.height).show(); var arrow = this.arrow, follow = this.follow, followOffset = follow.offset(), shellWidth = shell.outerWidth(), topOffset = (parseFloat(offset[0]) || 0), leftOffset = (parseFloat(offset[3]) || 0), top = followOffset.top + follow.height() + arrow.outerHeight() + topOffset, left = followOffset.left + (follow.outerWidth() - shellWidth) / 2 + leftOffset; arrow.css('left', shellWidth / 2 - leftOffset); shell.css('left', left).css('top', top); if (ops.animate) { shell.animate({ opacity: 1 }, ops.animateDuration); } return this; }, hide: function () { var ops = this.ops, shell = this.shell; if (ops.animate) { shell.stop().animate({ opacity: 0 }, ops.animateDuration, function () { shell.hide(); }); } else { shell.hide(); } return this; }, destroy: function () { this.shell.remove(); this.shell = this.contentShell = this.follow = null; } }; $.fn.tips = function (ops) { var self = this, _ops = { hoverTipsActiveShow: true, autoDisplay: false, follow: this, delay: 360 }; if (typeof ops === 'string') { _ops.content = ops; } else if (typeof ops === 'object') { _ops = $.extend(true, _ops, ops); } if (!_ops.content) { _ops.content = this.attr('data-tips') || ''; } var timer, evtNameWhitIn = 'mouseenter.ds_tips', evtNameWhitOut = 'mouseleave.ds_tips', tips = new ds.Tips(_ops), showTips = function () { clearTimeout(timer); tips.show(); }, hideTips = function () { clearTimeout(timer); timer = setTimeout(function () { tips.hide(); }, _ops.delay); }; if (_ops.hoverTipsActiveShow) { tips.shell.bind(evtNameWhitIn, showTips).bind(evtNameWhitOut, hideTips); } this.bind(evtNameWhitIn, showTips).bind(evtNameWhitOut, hideTips).data('tips', tips); var _destroy = tips.destroy; tips.destroy = function () { tips.shell.unbind(evtNameWhitIn, showTips).unbind(evtNameWhitOut, hideTips); self.unbind(evtNameWhitIn, showTips).unbind(evtNameWhitOut, hideTips).removeData('tips'); _destroy.apply(this, arguments); }; return this; }; })(this, jQuery);

/**
 * 全局导航
 * kongge@office.weiphone.com
 * 2013.05.16
*/
jQuery(function ($) {
    var shell = $('#gloabl_categorys');
    if (!shell.length) { return; }

    var
	panelDisplayed = false,
	subPanelDisplayed = false,
	supportOpacity = $.support.opacity,
	catWrap = $('#global_categorys_catwrap'),
	catPanel = shell.find('.nav_cats').eq(0),
	subPanel = shell.find('.nav_subcat_wrap').eq(0),
	panel = $('#global_categorys_inner').css('opacity', 0),
	panelMinWidth = panel.width(),
	panelMaxWidth = panelMinWidth + subPanel.width();

    var panelTimer;
    shell.hover(function () {
        clearTimeout(panelTimer);
        panelTimer = setTimeout(showCategory, 16);
    }, function () {
        clearTimeout(panelTimer);
        panelTimer = setTimeout(hideCategory, 360);
    });
    function showCategory() {
        if (supportOpacity) {
            panel.stop().show().animate({ opacity: 1 }, 200, function () {
                panelDisplayed = true;
            });
        }
        else {
            panel.show().css('opacity', 1);
            panelDisplayed = true;
        }
    }
    function hideCategory() {
        if (!panelDisplayed) { return; }
        if (supportOpacity) {
            panel.stop().animate({ opacity: 0 }, 320, function () {
                panel.hide().css('width', panelMinWidth);
                catWrap.css('overflow', 'hidden');
                if (prevSubCatTrigger) {
                    prevSubCatTrigger.removeClass('current');
                }
                subPanelDisplayed = false;
                panelDisplayed = false;
            });
        }
        else {
            panel.hide().css('width', panelMinWidth).css('opacity', 0);
            catWrap.css('overflow', 'hidden');
            if (prevSubCatTrigger) {
                prevSubCatTrigger.removeClass('current');
            }
            subPanelDisplayed = false;
            panelDisplayed = false;
        }
        catPanel.unbind('mousemove', pointHandler);
    };

    var
	subDelay = 160,
	catTimer, pointTimer,
	cachePoints = [0, 0, 0],
	currSubCat, prevSubCat,
	currSubCatTrigger, prevSubCatTrigger;
    catPanel.delegate('li', 'mouseenter', function (e) {
        clearTimeout(catTimer);
        var target, targetId = this.getAttribute('data-target');
        if (targetId && (target = $('#' + targetId)).length && prevSubCat !== target) {
            currSubCat = target;
            currSubCatTrigger = $(this);
            if (e.pageX > cachePoints[0]) {
                catTimer = setTimeout(showSubCat, subDelay);
            }
            else {
                showSubCat();
            }
        }
    });
    subPanel.bind('mouseenter', function () {
        clearTimeout(catTimer);
    });

    function pointHandler(e) {
        clearTimeout(pointTimer);
        pointTimer = setTimeout(function () {
            cachePoints.splice(0, 1);
            cachePoints.push(e.pageX);
        }, 16);
    }
    function showSubCat() {
        if (prevSubCat) {
            prevSubCatTrigger.removeClass('current');
            prevSubCat.hide();
        }
        if (!subPanelDisplayed) {
            subPanelDisplayed = true;
            ds.animate(panel[0], 'width', panelMaxWidth, 320, function () {
                catWrap.css('overflow', 'visible');
            });
            catPanel.bind('mousemove', pointHandler);
        }
        if (currSubCat.attr('data-parsed') !== '1') {
            currSubCat.attr('data-parsed', '1');
            currSubCat.html($('#' + currSubCat[0].id + '_tmpl').val());
        }
        currSubCatTrigger.addClass('current');
        prevSubCatTrigger = currSubCatTrigger;
        prevSubCat = currSubCat.show();
    }
});

/** 
 * 全局购物车
 * kongge@office.weiphone.com
 * 2013.05.27
**/
jQuery(function ($) {
    var shell = $('#global_user_cart');
    if (!shell.length) { return; }

    var
	timer,
	panel = shell.find('.user_cart_inner').eq(0).hide();
    shell.hover(function () {
        clearTimeout(timer);
        timer = setTimeout(function () {
            panel.animate({ opacity: 1 }, 200).slideDown(320, function () {
                panel.css('display', 'block');
            });
        }, 120);
    }, function () {
        clearTimeout(timer);
        timer = setTimeout(function () {
            panel.fadeOut();
        }, 360);
    });
});

/** 
 * 全局搜索
 * kongge@office.weiphone.com
 * 2012.04.26
**/
jQuery(function ($) {
    return;
    var
	prevPos = -1,
	global = window,
	rhtml = /<[^>]+>/g,
	schTxt = $('#sch_keyword'),
	prevText = schTxt.val(),
	schPanelDisplayed = false,
	schPanel = $('#sch_pre_panel').hide(),
	schItems = schPanel.find('a'),
	switchSch = function (dir) {
	    var el, len = schItems.length;
	    if (schPanelDisplayed && len > 0) {
	        if (prevPos >= 0 && prevPos < len) {
	            schItems[prevPos].className = '';
	        }
	        prevPos += dir < 0 ? -1 : 1;
	        prevPos = prevPos >= len ? 0 : prevPos < 0 ? len - 1 : prevPos;
	        el = schItems[prevPos];
	        prevText = el.innerHTML.replace(rhtml, '');
	        el.className = 'hover';
	        schTxt.val(prevText);
	    }
	},
	showSchPanel = function () {
	    if (!schPanelDisplayed) {
	        schPanelDisplayed = true;
	        schPanel.show();
	    }
	},
	_xhr = null,
	hideSchPanel = function () {
	    !!_xhr && _xhr.abort && _xhr.abort();
	    schPanelDisplayed = false;
	    schPanel.hide();
	},
	minLen = 1,
	maxLen = 8,
	baseUrl = global._baseUrl || '/',
	schUl = schPanel.find('ul').eq(0),
	_schUrl = baseUrl + 'catalogsearch/result/?q=',
	_url = baseUrl + 'catalogsearch/ajax/getSuggestDataAjaxJson/',
	_img = '<img alt="{1}" src="{0}" height="20" width="20" />',
	_tmpl = '<li><a href="{1}" title="{0}"><span class="pic">{2}</span>{0}</a></li>',
	_insertData = function (data) {
	    var i = 0, len = Math.min(maxLen, data.length), ret = [], d;
	    for (; i < len; i++) {
	        d = data[i];
	        ret.push(ds.mixStr(_tmpl, d.title, _schUrl + encodeURIComponent(d.title), !!d.photo_url ? ds.mixStr(_img, d.photo_url, d.title) : ''));
	    }
	    if (len >= minLen) {
	        schUl.html(ret.join(''));
	        showSchPanel();
	    }
	    else {
	        hideSchPanel();
	    }
	},
	_cache = {},
	getData = function (txt, callback) {
	    !!_xhr && _xhr.abort && _xhr.abort();
	    if (!txt && ('' + txt).length < 1) return;
	    callback = $.type(callback) === 'function' ? callback : _insertData;
	    if (!!_cache[txt]) return callback.call(_cache[txt], _cache[txt].info);
	    _xhr = $.get(_url, { q: txt || schTxt.val() }, function (ret) {
	        if (ret.success != '1') return hideSchPanel();
	        schItems[prevPos] && (schItems[prevPos].className = '');
	        callback.call(ret, ret.info);
	        schItems = schPanel.find('a');
	        _cache[txt] = ret;
	        prevPos = -1;
	    }, 'json');
	};
    schTxt.keyup(function (e) {
        var code = e.keyCode;
        if ((code === 38 || code === 40) && schPanel.css('display') === 'block') {
            switchSch(code - 39);
        }
        else if (code === 27) {
            hideSchPanel();
        }
        else if (prevText != schTxt.val()) {
            !!(prevText = schTxt.val()) ? getData(prevText) : hideSchPanel();
        }
    })
	//stop click bubble
	.parent().click(function (e) {
	    e.stopPropagation();
	});
    $(document).click(hideSchPanel);
    //default AD
    var _defAd, ads = (schTxt.attr('data-default') || '').split(',');
    if (ads.length > 0) {
        _defAd = ads[parseInt(ds.getRndNum(0, ads.length), 10)];
        schTxt.focus(function () {
            if (this.value === _defAd) {
                this.value = '';
                this.className = '';
            }
        })
		.blur(function () {
		    if (this.value === '') {
		        this.value = _defAd;
		        this.className = 'default';
		    }
		});
        schTxt.val() == '' && schTxt.val(_defAd).addClass('default');
    }
});

/**
 * topbar - 加入收藏 & IE6 fix pull_links
 * kongge@office.weiphone.com
 * 2012.04.26
*/
jQuery(function ($) {
    var
	global = window,
	baseUrl = global._baseUrl || 'http://www.gyt1993.com/',
	addFav = function (title, url, onsucces, onerror) {
	    onsucces = onsucces || ds.noop;
	    if (global.sidebar && global.sidebar.addPanel) {
	        global.sidebar.addPanel(title, url, '');
	        onsucces.call(null);
	    }
	    else {
	        try {
	            global.external.addFavorite(url, title);
	            onsucces.call(null);
	        }
	        catch (_) {
	            (onerror || ds.noop).call(null);
	        }
	    }
	};
    var shell = $('#global_topbar');
    shell.delegate('#add_to_favorites', 'click', function (e) {
        e.preventDefault();
        addFav('宝氏照明商城 - 吉安最大的网上商城', baseUrl, ds.noop, function () {
            ds.dialog.alert('收藏失败，请按Ctrl+D进行操作');
        });
    });
    //IE6 fix pull_links
    if (ds.isIE6) {
        shell.delegate('.pull_link', 'mouseenter', function () {
            this.className += ' hover';
        })
		.delegate('.pull_link', 'mouseleave', function () {
		    this.className = this.className.replace(/\s+hover\b/g, '');
		});
    }
});

//问卷调查
jQuery(function ($) {
    $('.survey_tab').delegate('a', 'click', function (event) {
        event.preventDefault();

        var id = $(this).attr('href');
        if (id) {
            ds.scrollTo($(id).offset().top);
        }
    });
});

//关注我们
jQuery(function ($) {
    setTimeout(function () {
        $('#follow_us').html('<span id="follow_by_sina" style="float:left;height:22px;"><iframe src="http://widget.weibo.com/relationship/followbutton.php?language=zh_cn&width=124&height=24&uid=2839365622&style=2&btn=red&dpc=1" width="124" height="22" frameborder="0" allowtransparency="true" scrolling="no" marginheight="0" marginwidth="0"></iframe></span> ');
    }, 320);
});

//deprecated
var wep = {
    dialog: function () {
        return ds.dialog.apply(ds, arguments);
    }
};