/**
 * Module for displaying "Waiting for..." dialog using Bootstrap
 *
 * @author Eugene Maslovich <ehpc@em42.ru>
 */

var esperaDialogo = esperaDialogo || (function ($) {
    'use strict';

    // Creating modal dialog's DOM
    var $dialog = $(
		'<div id="waitingDialog_Modal" class="modal" data-backdrop="static" data-keyboard="false" tabindex="-1" role="dialog" aria-hidden="true" style="padding-top:15%; overflow-y:visible;">' +
		'   <div id="waitingDialog_Dialog" class="modal-dialog modal-m">' +
		'       <div id="waitingDialog__Content" class="modal-content">' +
		'           <div class="modal-header">' +
        '               <h4 style="margin:0;"><b id="waitingDialog_Header"></b></h3>' +
        '           </div>' +
		'           <div id="waitingDialog_Body" class="modal-body">' +
		'               <div class="progress progress-striped active" style="margin-bottom:0;">' +
        '                   <div id="waitingDialog_Bar" class="progress-bar" style="width: 100%"></div>' +
        '               </div>' +
		'           </div>' +
		'       </div>'+
        '   </div>'+
        '</div>');

    return {
        /**
		 * Opens our dialog
		 * @param message Custom message
		 * @param options Custom options:
		 * 				  options.dialogSize - bootstrap postfix for dialog size, e.g. "sm", "m";
		 * 				  options.progressType - bootstrap postfix for progress bar type, e.g. "success", "warning".
		 */
        mostrar: function (message, options) {
            // Assigning defaults
            if (typeof options === 'undefined') {
                options = {};
            }
            if (typeof message === 'undefined') {
                message = 'Loading';
            }
            var settings = $.extend({
                dialogSize: 'm',
                progressType: '',
                onHide: null // This callback runs after the dialog was hidden
            }, options);

            // Configuring dialog
            $dialog.find('#waitingDialog_Dialog').attr('class', 'modal-dialog').addClass('modal-' + settings.dialogSize);
            $dialog.find('#waitingDialog_Bar').attr('class', 'progress-bar');
            if (settings.progressType) {
                $dialog.find('#waitingDialog_Bar').addClass('progress-bar-' + settings.progressType);
            }
            $dialog.find('#waitingDialog_Header').text(message);

            // Adding callbacks
            if (typeof settings.onHide === 'function') {
                $dialog.off('hidden.bs.modal').on('hidden.bs.modal', function (e) {
                    settings.onHide.call($dialog);
                });
            }
            // Opening dialog
            $dialog.modal();
        },
        /**
		 * Closes dialog
		 */
        ocultar: function () {
            $dialog.modal('hide');
        }
    };

})(jQuery);