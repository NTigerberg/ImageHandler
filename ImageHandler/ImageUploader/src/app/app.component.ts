import { Component } from '@angular/core';
import { Headers, Http } from '@angular/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  constructor(public http: Http	) {

  }

  title = 'Image Uploader';
  public url = 'http://localhost:35195/api/image/upload';

  onUploadFinished($event) {
    debugger;
    console.log($event + 'Finished');
  }
  // uploadImage() {
  //   return this.http.get(url).toPromise()
  //   .then(response => response.json() as T);
  // }
}
