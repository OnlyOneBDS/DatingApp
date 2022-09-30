import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.css']
})
export class TestErrorsComponent implements OnInit {
  baseUrl = "https://localhost:7018/api/";
  validationErrors: string[] = [];

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }

  get400Error() {
    return this.http
      .get(this.baseUrl + 'buggy/bad-request')
      .subscribe({
        next: (response) => console.log(response),
        error: (e) => console.log(e)
      });
  }

  get400ValidationError() {
    return this.http
      .post(this.baseUrl + 'account/register', {})
      .subscribe({
        next: (response) => console.log(response),
        error: (e) => {
          console.log(e);
          this.validationErrors = e;
        }
      });
  }

  get401Error() {
    return this.http
      .get(this.baseUrl + 'buggy/auth')
      .subscribe({
        next: (response) => console.log(response),
        error: (e) => console.log(e)
      });
  }

  get404Error() {
    return this.http
      .get(this.baseUrl + 'buggy/not-found')
      .subscribe({
        next: (response) => console.log(response),
        error: (e) => console.log(e)
      });
  }

  get500Error() {
    return this.http
      .get(this.baseUrl + 'buggy/server-error')
      .subscribe({
        next: (response) => console.log(response),
        error: (e) => console.log(e)
      });
  }
}
