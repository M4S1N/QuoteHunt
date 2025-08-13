import { Injectable } from "@angular/core";
import { FormGroup } from "@angular/forms";

@Injectable({
    providedIn: 'root',
})
export class FormDataService {
    constructor() { }

    toFormData(formGroup: FormGroup): FormData {
        const formData = new FormData();
        const controls = formGroup.controls;

        for (const key in controls) {
            if (controls.hasOwnProperty(key)) {
                const control = controls[key];
                
                if (control.value instanceof File) {
                    formData.append(key, control.value);
                } else {
                    formData.append(key, control.value || '');
                }
            }
        }
        
        return formData;
    }
}