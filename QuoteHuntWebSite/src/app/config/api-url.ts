import { Injectable } from "@angular/core";

@Injectable({
    providedIn: "root"
})
export class UrlService {

    constructor() {}

    getQuote = (page?: string | null, tag?: string | null): string => {
        let url = `/api/Quote`;
        if (!!page) {
            url += `?page=${page}`;
        }
        if (!!tag) {
            url += `&tag=${tag}`;
        }
        return url;
    }
}