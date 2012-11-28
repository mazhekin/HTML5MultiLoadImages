$(function () {
    var fileInput = $('#file-field');
    var imgList = $('ul#img-list');
    var dropBox = $('#img-container');

    fileInput.bind({
        change: function () {
            displayFiles(this.files);
        }
    });

    // drag and drop events while moving files to element dropBox
    dropBox.bind({
        dragenter: function () {
            $(this).addClass('highlighted');
            return false;
        },
        dragover: function () {
            return false;
        },
        dragleave: function () {
            $(this).removeClass('highlighted');
            return false;
        },
        drop: function (e) {
            var dt = e.originalEvent.dataTransfer;
            displayFiles(dt.files);
            return false;
        }
    });

    function displayFiles(files) {
        $.each(files, function (i, file) {
            if (!file.type.match(/image.*/)) {
                // Отсеиваем не картинки
                return true;
            }
            // Создаем элемент li и помещаем в него название, миниатюру и progress bar,
            // а также создаем ему свойство file, куда помещаем объект File (при загрузке понадобится)
            var li = $('<li/>').appendTo(imgList);
            $('<div/>').text(file.name).appendTo(li);
            var img = $('<img/>').appendTo(li);
            $('<div/>').addClass('progress').text('0%').appendTo(li);
            li.get(0).file = file;

            // Создаем объект FileReader и по завершении чтения файла, отображаем миниатюру и обновляем
            // инфу обо всех файлах
            var reader = new FileReader();
            reader.onload = (function (aImg) {
                return function (e) {
                    aImg.attr('src', e.target.result);
                    aImg.attr('width', 150);
                    /* ... обновляем инфу о выбранных файлах ... */
                };
            })(img);

            reader.readAsDataURL(file);
        });
    }

});