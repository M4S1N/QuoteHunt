import { HttpClient, HttpErrorResponse, HttpHeaders } from "@angular/common/http";
import { Injector } from "@angular/core";
import { environment } from "../../../environments/environment";
import { Observable } from "rxjs";

export abstract class BaseService {

    protected apiUrl = environment.apiUrl;

    protected constructor(
        protected http: HttpClient
    ) { }

    protected getHeaders(): HttpHeaders {
        return new HttpHeaders();
    }

    // Shared function for making GET requests
    protected get(endpoint: string, body?: any): Observable<any> {
        const queryParams = this.formatQueryParams(body);

        return this.http.get(`${this.apiUrl}${endpoint}${queryParams}`, {
            headers: this.getHeaders()
        });
    } 
    
    // Shared function for making POST requests
    protected post(endpoint: string, body?: any): Observable<any> {
        return this.http.post(`${this.apiUrl}${endpoint}`, body, {
            headers: this.getHeaders()
        });
    }

    // Shared function for making PUT requests
    protected put(endpoint: string, body?: any): Observable<any> {
        return this.http.put(`${this.apiUrl}${endpoint}`, body, {
            headers: this.getHeaders()
        });
    }

    // Shared function for making DELETE requests
    protected delete(endpoint: string, body?: any): Observable<any> {
        const queryParams = this.formatQueryParams(body);

        return this.http.delete(`${this.apiUrl}${endpoint}${queryParams}`, {
            headers: this.getHeaders()
        });
    }

    private formatQueryParams(data: Record<string, any>): string {
        let params = new URLSearchParams();
        if (!!data) {
            Object.entries(data)
                .filter(([_, value]) => !!value)
                .forEach(([key, value]) => {
                    if (value instanceof Array) value.forEach(x => params.append(key, x));
                    else params.append(key, value.toString())
                });
        }
        const queryParams = params.toString();
        return queryParams.length > 0 ? `?${queryParams}` : '';
    }
} 