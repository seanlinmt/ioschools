/**
 * Internationalization plugin
 * 
 * Depends on jWYSIWYG
 */
(function ($) {
	if (undefined === $.wysiwyg) {
		throw "wysiwyg.i18n.js depends on $.wysiwyg";
	}

	/*
	 * Wysiwyg namespace: public properties and methods
	 */
	var i18n = {
		name: "i18n",
		version: "",
		defaults: {
			lang: "en",			// change to your language by passing lang option
			wysiwygLang: "en"	// default WYSIWYG language
		},
		lang: {},
		options: {},

		init: function (Wysiwyg, lang) {
			if (!Wysiwyg.options.i18n) {
				return true;
			}

			if (!lang) {
				lang = Wysiwyg.options.i18n;
			}

			if ((lang !== this.defaults.wysiwygLang) && (undefined === $.wysiwyg.i18n.lang[lang])) {
				if ($.wysiwyg.autoload) {
					$.wysiwyg.autoload.lang("lang." + lang + ".js", function () {
						$.wysiwyg.i18n.init(Wysiwyg, lang);
					});
				} else {
					throw 'Language "' + lang + '" not found in $.wysiwyg.i18n. You need to include this language file';
				}
			}

			this.options.lang = lang;

			this.translateControls(Wysiwyg);
		},

		translateControls: function (Wysiwyg) {
			Wysiwyg.ui.toolbar.find("li").each(function () {
				if (Wysiwyg.controls[$(this).attr("class")]) {
					$(this).attr("title", $.wysiwyg.i18n.t(Wysiwyg.controls[$(this).attr("class")].tooltip, "controls"));
				}
			});
		},

		run: function (object, lang) {
			if ("object" !== typeof (object) || !object.context) {
				object = this;
			}

			if (!object.each) {
				console.error($.wysiwyg.messages.noObject);
			}

			return object.each(function () {
				var oWysiwyg = $(this).data("wysiwyg");

				if (!oWysiwyg) {
					return this;
				}

				$.wysiwyg.i18n.init(oWysiwyg, lang);
			});
		},

		t: function (phrase, section, lang) {
			var i, section_array, object;

			if (!lang) {
				lang = this.options.lang;
			}

			if ((lang === this.defaults.wysiwygLang) || (!this.lang[lang])) {
				return phrase;
			}

			object = this.lang[lang];
			section_array = section.split(".");
			for (i = 0; i < section_array.length; i += 1) {
				if (object[section_array[i]]) {
					object = object[section_array[i]];
				}
			}

			if (object[phrase]) {
				return object[phrase];
			}

			return phrase;
		}
	};

	$.wysiwyg.plugin.register(i18n);
	$.wysiwyg.plugin.listen("initFrame", "i18n.init");
})(jQuery);
