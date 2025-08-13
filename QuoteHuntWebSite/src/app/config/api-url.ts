import { Injectable } from "@angular/core";

@Injectable({
    providedIn: "root"
})
export class UrlService {

    constructor() {}

    getQuote = (page: string = "1", tag?: string): string => {
        let url = `api/Quote?page=${page}`;
        if (tag) {
            url += `&tag=${tag}`;
        }
        return url;
    }
}