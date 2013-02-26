var DateTimePicker = {
	
	index : 0,
	
	pickers : [],

	configure : function(e){
		
		var index = DateTimePicker.index++;
		var oCalendarMenu = new YAHOO.widget.Overlay("calendarmenu"+index);	
		var oText = document.createElement("input");
		oText.type = "text";
		oText.name = $(e).getAttribute('name');
		if ($(e).getAttribute('nameasid')=="1")
			oText.id = oText.name;
		else
			oText.id = "datepick"+index;
		oText.value = $(e).getAttribute('value');
		oText.setAttribute('size', 10);
		$(e).appendChild(oText);
        var oButton = new YAHOO.widget.Button(
			{type:"menu", 
			label:"choose",
			id:"calendarpicker"+index, 
			menu: oCalendarMenu, 
			container: e });
		var picker = {
			textField : oText,
			oCalendarMenu : oCalendarMenu,
			oButton : oButton,
            onButtonClick : function() {
					this.oCalendarMenu.picker = this;
					this.oCalendarMenu.setBody("&#32;");
					this.oCalendarMenu.body.id = "calendarcontainer"+index;
					this.oCalendarMenu.render($(e));
					this.oCalendarMenu.align();
					var oCalendar = new YAHOO.widget.Calendar("buttoncalendar"+index, this.oCalendarMenu.body.id);
					oCalendar.render();
					var forShow = (function(){this.oCalendarMenu.show()}).bind(this);
					oCalendar.changePageEvent.subscribe(function () {
		               
						window.setTimeout(function () {
							forShow();
						}, 0);
		            
					});
					oCalendar.selectEvent.subscribe(function (p_sType, p_aArgs) {

						var aDate;

						if (p_aArgs) {
		                        
							aDate = p_aArgs[0][0];
		                    this.textField.value  = aDate[2]+'.'+aDate[1]+'.'+ aDate[0];

						}
		                
						this.oCalendarMenu.hide();
		            
					}.bind(this));
		            

					/*
						 Unsubscribe from the "click" event so that this code is 
						 only executed once
					*/

					this.oButton.unsubscribe("click", this.onButtonClick);
		        


			}
              
                                            
		};
		picker.onButtonClick = picker.onButtonClick.bind(picker);
		picker.oButton.on('click',picker.onButtonClick);
		
		DateTimePicker.pickers.push(picker);
        return picker;
	}
}