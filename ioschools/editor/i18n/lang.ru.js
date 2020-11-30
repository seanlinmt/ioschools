/**
 * Internationalization: Russian language
 * 
 * Depends on jWYSIWYG, $.wysiwyg.i18n
 * 
 * By: frost-nzcr4 on github.com
 */
(function ($) {
	if (undefined === $.wysiwyg) {
		throw "lang.ru.js depends on $.wysiwyg";
	}
	if (undefined === $.wysiwyg.i18n) {
		throw "lang.ru.js depends on $.wysiwyg.i18n";
	}

	$.wysiwyg.i18n.lang.ru = {
		controls: {
			"Bold": "Жирный",
			"Colorpicker": "Выбор цвета",
			"Copy": "Копировать",
			"Create link": "Создать ссылку",
			"Cut": "Вырезать",
			"Decrease font size": "Уменьшить шрифт",
			"Fullscreen": "На весь экран",
			"Header 1": "Заголовок 1",
			"Header 2": "Заголовок 2",
			"Header 3": "Заголовок 3",
			"View source code": "Посмотреть исходный код",
			"Increase font size": "Увеличить шрифт",
			"Indent": "Отступ",
			"Insert Horizontal Rule": "Вставить горизонтальную прямую",
			"Insert image": "Вставить изображение",
			"Insert Ordered List": "Вставить нумерованный список",
			"Insert table": "Вставить таблицу",
			"Insert Unordered List": "Вставить ненумерованный список",
			"Italic": "Курсив",
			"Justify Center": "Выровнять по центру",
			"Justify Full": "Выровнять по ширине",
			"Justify Left": "Выровнять по левой стороне",
			"Justify Right": "Выровнять по правой стороне",
			"Left to Right": "Слева направо",
			"Outdent": "Убрать отступ",
			"Paste": "Вставить",
			"Redo": "Вернуть действие",
			"Remove formatting": "Убрать форматирование",
			"Right to Left": "Справа налево",
			"Strike-through": "Зачёркнутый",
			"Subscript": "Нижний регистр",
			"Superscript": "Верхний регистр",
			"Underline": "Подчёркнутый",
			"Undo": "Отменить действие"
		},

		dialogs: {
			// for all
			"Apply": "Применить",
			"Cancel": "Отмена",

			colorpicker: {
				"Colorpicker": "Выбор цвета",
				"Color": "Цвет"
			},

			image: {
				"Insert Image": "Вставить изображение",
				"Preview": "Просмотр",
				"URL": "URL адрес",
				"Title": "Название",
				"Description": "Альт. текст",
				"Width": "Ширина",
				"Height": "Высота",
				"Original W x H": "Оригинальные Ш x В",
				"Float": "Положение",
				"None": "Не выбрано",
				"Left": "Слева",
				"Right": "Справа"
			},

			link: {
				"Insert Link": "Вставить ссылку",
				"Link URL": "URL адрес",
				"Link Title": "Название",
				"Link Target": "Цель"
			},

			table: {
				"Insert table": "Вставить таблицу",
				"Count of columns": "Кол-во колонок",
				"Count of rows": "Кол-во строк"
			}
		}
	};
})(jQuery);