
(function (jQuery) {

    $(function () {

        var my = my || {};

        ko.bindingHandlers.imgScr = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                var img = $(element);
                var file = valueAccessor();
                var model = bindingContext.$root;

                // Upload here file to server and the display it
                var reader = new FileReader();
                reader.onloadend = function () {
                    model.uploadFile(file, "/Home/Upload/", reader, img);
                };
                reader.readAsBinaryString(file);
            }
        };

        my.vm = (function () {
            var filesToSave = [],
                savedFiles = ko.observableArray(),
                filesChanged = function (data, event) {
                    var self = this;
                    $.each(event.srcElement.files, function (i, file) {
                        if (!file.type.match(/image.*/)) {
                            return true;
                        }
                        filesToSave.push(file);
                    });
                    if (filesToSave.length > 0) {
                        self.savedFiles.unshift(filesToSave.shift());
                    }
                },
                uploadFile = function (file, url, reader, img) {
                    var self = this,
                        xhr = new XMLHttpRequest(),
                        boundary = '------multipartformboundary' + (new Date).getTime();

                    xhr.open("POST", url, true);
                    xhr.setRequestHeader('content-type', 'multipart/form-data; boundary=' + boundary);
                    xhr.setRequestHeader("content-length", file.size);
                    xhr.setRequestHeader("cache-control", "no-cache");

                    xhr.onreadystatechange = function () {
                        if (this.readyState == 4) {
                            if (this.status == 200) {
                                onsuccess(img, jQuery.parseJSON(this.response));
                            } else {
                                onerror();
                            }
                            // uploaded or not previous, but need to upload next image
                            if (filesToSave.length > 0) {
                                self.savedFiles.unshift(filesToSave.shift());
                            }
                        }
                    };

                    var body = getBody(file.name, reader.result, boundary);

                    try {
                        if (xhr.sendAsBinary) {
                            // firefox
                            xhr.sendAsBinary(body);
                        } else {
                            // chrome (according W3C specification)
                            function byteValue(x) {
                                return x.charCodeAt(0) & 0xff;
                            }
                            var ords = Array.prototype.map.call(body, byteValue);
                            var ui8a = new Uint8Array(ords);
                            xhr.send(ui8a.buffer);
                        }
                    } catch (e) { }
                },
                getBody = function (filename, filedata, boundary) {
                    var dashdash = '--',
                        crlf = '\r\n',
                        body = '';

                    body += dashdash;
                    body += boundary;
                    body += crlf;
                    body += 'Content-Disposition: form-data; name="pic"; filename="' + filename + '"';
                    body += crlf;

                    body += 'Content-Type: application/octet-stream';
                    body += crlf;
                    body += crlf;

                    body += filedata;
                    body += crlf;

                    body += dashdash;
                    body += boundary;
                    body += dashdash;
                    body += crlf;
                    return body;
                },
                onsuccess = function (img, response) {
                    $(img).attr('src', '/home/icon/' + response.file);
                },
                onerror = function () {
                    alert('error');
                };
            return {
                savedFiles: savedFiles,
                filesChanged: filesChanged,
                uploadFile: uploadFile
            }
        })();

        ko.applyBindings(my.vm);

    });


})(jQuery);

